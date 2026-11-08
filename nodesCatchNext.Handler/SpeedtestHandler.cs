using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using nodesCatchNext.Mode;

namespace nodesCatchNext.Handler;

public class SpeedtestHandler
{
	private readonly Config _config;

	private readonly MihomoHandler _MihomoHandler;

	private readonly List<int> _selecteds;

	private readonly Action<VmessItem, string, string, bool> _updateDelayFunc;

	private readonly Action<VmessItem, string, string, bool> _updateSpeedFunc;

	private readonly Action _onCompleted;

	private readonly Dictionary<int, VmessItem> _nodeSnapshot;

	public SpeedtestHandler(
		Config config,
		MihomoHandler MihomoHandler,
		List<int> selecteds,
		Action<VmessItem, string, string, bool> updateDelayFunc,
		Action<VmessItem, string, string, bool> updateSpeedFunc,
		Action onCompleted)
	{
		_config = config;
		_MihomoHandler = MihomoHandler;
		_selecteds = Utils.DeepCopy(selecteds);
		_updateDelayFunc = updateDelayFunc;
		_updateSpeedFunc = updateSpeedFunc;
		_onCompleted = onCompleted;
		_nodeSnapshot = new Dictionary<int, VmessItem>();
		foreach (int selected in _selecteds)
		{
			if (selected >= 0 && selected < _config.vmess.Count)
			{
				_nodeSnapshot[selected] = _config.vmess[selected];
			}
		}
	}

	/// <summary>
	/// 并发测试所有选中节点的延迟（无线程数限制，支持多次测试并计算最低延迟与平均延迟，每测完一个节点立即排序）
	/// </summary>
	public void RunDelayTest(CancellationToken ct)
	{
		try
		{
			int httpPort = _config.GetLocalPort("speedtest");
			if (!double.TryParse(_config.Timeout, out var timeoutSec) || timeoutSec <= 0)
			{
				timeoutSec = 5.0;
			}
			int timeOutMs = (int)(timeoutSec * 1000.0);
			if (!int.TryParse(_config.PingCount, out var pingCount) || pingCount <= 0)
			{
				pingCount = 3;
			}
			string delayUrl = string.IsNullOrWhiteSpace(_config.speedPingTestUrl)
				? "https://www.cloudflare.com/cdn-cgi/trace"
				: _config.speedPingTestUrl.Trim();

			ServicePointManager.DefaultConnectionLimit = Math.Max(ServicePointManager.DefaultConnectionLimit, 2048);
			ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);
			ThreadPool.SetMinThreads(Math.Max(workerThreads, 256), Math.Max(completionPortThreads, 256));

			List<Task> tasks = new List<Task>(_selecteds.Count);
			foreach (int selected in _selecteds)
			{
				int originalIndex = selected;
				tasks.Add(Task.Run(() =>
				{
					if (!_nodeSnapshot.TryGetValue(originalIndex, out var node) || node == null)
					{
						return;
					}
					if (ct.IsCancellationRequested)
					{
						_updateDelayFunc?.Invoke(node, "测速被取消", "测速被取消", true);
						return;
					}

					_updateDelayFunc?.Invoke(node, "正在测试...", "正在测试...", false);

					List<int> validDelays = new List<int>(pingCount);
					string lastError = "超时";
					int proxyPortName = httpPort + originalIndex;
					string requestUrl = $"http://{_config.externalController}/proxies/{proxyPortName}/delay?timeout={timeOutMs}&url={Uri.EscapeDataString(delayUrl)}";

					for (int i = 0; i < pingCount; i++)
					{
						if (ct.IsCancellationRequested)
						{
							break;
						}
						string rawResp = GetRealPingTime(requestUrl, timeOutMs * 2 + 1000);
						if (TryParseDelayMs(rawResp, out var delayMs, out var errMsg))
						{
							validDelays.Add(delayMs);
							if (i < pingCount - 1)
							{
								int minVal = validDelays.Min();
								int avgVal = (int)Math.Round(validDelays.Average());
								_updateDelayFunc?.Invoke(node, $"测试中 {minVal}ms", $"测试中 {avgVal}ms", false);
							}
						}
						else
						{
							lastError = errMsg;
						}
					}

					if (validDelays.Count > 0)
					{
						int minVal = validDelays.Min();
						int avgVal = (int)Math.Round(validDelays.Average());
						_updateDelayFunc?.Invoke(node, $"{minVal}ms", $"{avgVal}ms", true);
					}
					else if (ct.IsCancellationRequested)
					{
						_updateDelayFunc?.Invoke(node, "测速被取消", "测速被取消", true);
					}
					else
					{
						_updateDelayFunc?.Invoke(node, lastError, lastError, true);
					}
				}));
			}

			Task.WaitAll(tasks.ToArray());
		}
		catch
		{
		}
		finally
		{
			_MihomoHandler?.ShowMsg(updateToTrayTooltip: false, ct.IsCancellationRequested ? "延迟测试已停止" : "延迟测试完成！");
			_onCompleted?.Invoke();
		}
	}

	/// <summary>
	/// 并发测试所有选中节点的下载速度（按测试线程数同时并发测试多个节点、支持最大时间控制，每测完一个节点立即排序）
	/// </summary>
	/// <summary>
	/// 并发对所有目标节点直接执行 TCPing 测试（无需启动代理核心，直接建立 TCP 握手测量 RTT）
	/// </summary>
	public void RunTcpingTest(CancellationToken ct)
	{
		try
		{
			if (!double.TryParse(_config.Timeout, out var timeoutSec) || timeoutSec <= 0)
			{
				timeoutSec = 5.0;
			}
			int timeOutMs = (int)(timeoutSec * 1000.0);
			if (!int.TryParse(_config.PingCount, out var pingCount) || pingCount <= 0)
			{
				pingCount = 3;
			}

			ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);
			ThreadPool.SetMinThreads(Math.Max(workerThreads, 256), Math.Max(completionPortThreads, 256));

			List<Task> tasks = new List<Task>(_selecteds.Count);
			foreach (int selected in _selecteds)
			{
				int originalIndex = selected;
				tasks.Add(Task.Run(async () =>
				{
					if (!_nodeSnapshot.TryGetValue(originalIndex, out var node) || node == null)
					{
						return;
					}
					if (ct.IsCancellationRequested)
					{
						_updateDelayFunc?.Invoke(node, "测速被取消", "测速被取消", true);
						return;
					}

					string host = node.address?.Trim();
					int port = node.port;

					if (string.IsNullOrEmpty(host) || port <= 0 || port > 65535)
					{
						_updateDelayFunc?.Invoke(node, "无效地址", "无效地址", true);
						return;
					}

					// 去除可能误填的 url 前缀与尾部斜杠
					if (host.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) host = host.Substring(7);
					else if (host.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) host = host.Substring(8);
					int slashIdx = host.IndexOf('/');
					if (slashIdx >= 0) host = host.Substring(0, slashIdx);

					_updateDelayFunc?.Invoke(node, "正在测试...", "正在测试...", false);

					List<int> validDelays = new List<int>(pingCount);
					string lastError = "超时";

					for (int i = 0; i < pingCount; i++)
					{
						if (ct.IsCancellationRequested)
						{
							break;
						}

						var (success, delayMs, errMsg) = await SingleTcpPingAsync(host, port, timeOutMs, ct).ConfigureAwait(false);
						if (success)
						{
							validDelays.Add(delayMs);
							if (i < pingCount - 1)
							{
								int minVal = validDelays.Min();
								int avgVal = (int)Math.Round(validDelays.Average());
								_updateDelayFunc?.Invoke(node, $"测试中 {minVal}ms", $"测试中 {avgVal}ms", false);
							}
						}
						else
						{
							lastError = errMsg ?? "失败";
						}

						if (i < pingCount - 1 && !ct.IsCancellationRequested)
						{
							await Task.Delay(50, ct).ConfigureAwait(false);
						}
					}

					if (validDelays.Count > 0)
					{
						int minVal = validDelays.Min();
						int avgVal = (int)Math.Round(validDelays.Average());
						_updateDelayFunc?.Invoke(node, $"{minVal}ms", $"{avgVal}ms", true);
					}
					else if (ct.IsCancellationRequested)
					{
						_updateDelayFunc?.Invoke(node, "测速被取消", "测速被取消", true);
					}
					else
					{
						_updateDelayFunc?.Invoke(node, lastError, lastError, true);
					}
				}, ct));
			}

			Task.WaitAll(tasks.ToArray());
		}
		catch
		{
		}
		finally
		{
			_MihomoHandler?.ShowMsg(updateToTrayTooltip: false, ct.IsCancellationRequested ? "直接TCPing测试已停止" : "直接TCPing测试完成！");
			_onCompleted?.Invoke();
		}
	}

	/// <summary>
	/// 单次 TCPing 握手测量
	/// </summary>
	private static async Task<(bool success, int delayMs, string error)> SingleTcpPingAsync(string host, int port, int timeoutMs, CancellationToken ct)
	{
		var sw = Stopwatch.StartNew();
		Socket socket = null;
		try
		{
			IPAddress ip;
			if (!IPAddress.TryParse(host, out ip))
			{
				try
				{
					var dnsTask = System.Net.Dns.GetHostAddressesAsync(host);
					if (await Task.WhenAny(dnsTask, Task.Delay(timeoutMs, ct)).ConfigureAwait(false) != dnsTask)
					{
						return (false, -1, ct.IsCancellationRequested ? "已取消" : "DNS解析超时");
					}
					var addresses = await dnsTask.ConfigureAwait(false);
					if (addresses == null || addresses.Length == 0)
					{
						return (false, -1, "DNS解析失败");
					}
					ip = addresses[0];
				}
				catch (OperationCanceledException)
				{
					return (false, -1, "已取消");
				}
				catch (Exception)
				{
					return (false, -1, "域名解析失败");
				}
			}

			socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
			{
				Blocking = false
			};

			sw.Restart();
			var connectTask = Task.Factory.FromAsync(
				socket.BeginConnect,
				socket.EndConnect,
				ip,
				port,
				null);

			var timeoutTask = Task.Delay(timeoutMs, ct);
			var completed = await Task.WhenAny(connectTask, timeoutTask).ConfigureAwait(false);

			if (completed == connectTask)
			{
				await connectTask.ConfigureAwait(false);
				sw.Stop();
				return (true, (int)sw.ElapsedMilliseconds, null);
			}
			else
			{
				try { socket.Close(); } catch { }
				if (ct.IsCancellationRequested)
				{
					return (false, -1, "已取消");
				}
				return (false, -1, "超时");
			}
		}
		catch (OperationCanceledException)
		{
			return (false, -1, "已取消");
		}
		catch (SocketException ex)
		{
			if (ex.SocketErrorCode == SocketError.ConnectionRefused)
			{
				return (false, -1, "拒绝连接");
			}
			if (ex.SocketErrorCode == SocketError.TimedOut)
			{
				return (false, -1, "超时");
			}
			return (false, -1, "连接失败");
		}
		catch (Exception)
		{
			return (false, -1, "失败");
		}
		finally
		{
			if (socket != null)
			{
				try
				{
					socket.Close();
					socket.Dispose();
				}
				catch { }
			}
		}
	}

	public void RunDownloadTest(CancellationToken ct)
	{
		try
		{
			if (!int.TryParse(_config.DownloadThread, out var threadCount) || threadCount <= 0)
			{
				threadCount = 4;
			}
			if (threadCount > 128)
			{
				threadCount = 128;
			}
			if (!int.TryParse(_config.DownloadMaxTime, out var maxTimeSec) || maxTimeSec <= 0)
			{
				maxTimeSec = 10;
			}
			string speedTestUrl = string.IsNullOrWhiteSpace(_config.speedTestUrl)
				? "https://cdn.kernel.org/pub/linux/kernel/v6.x/linux-6.2.15.tar.xz"
				: _config.speedTestUrl.Trim();

			int localPort = _config.GetLocalPort("speedtest");

			Utils.SetSecurityProtocol();
			ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
			ServicePointManager.DefaultConnectionLimit = Math.Max(ServicePointManager.DefaultConnectionLimit, 2048);
			ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);
			ThreadPool.SetMinThreads(Math.Max(workerThreads, threadCount * 4 + 32), Math.Max(completionPortThreads, threadCount * 4 + 32));

			using SemaphoreSlim semaphore = new SemaphoreSlim(threadCount, threadCount);
			List<Task> nodeTasks = new List<Task>(_selecteds.Count);

			foreach (int selected in _selecteds)
			{
				int originalIndex = selected;
				nodeTasks.Add(Task.Run(() =>
				{
					if (!_nodeSnapshot.TryGetValue(originalIndex, out var node) || node == null)
					{
						return;
					}
					bool acquired = false;
					try
					{
						semaphore.Wait(ct);
						acquired = true;
					}
					catch (OperationCanceledException)
					{
						_updateSpeedFunc?.Invoke(node, "测速被取消", "", true);
						return;
					}

					try
					{
						if (ct.IsCancellationRequested)
						{
							_updateSpeedFunc?.Invoke(node, "测速被取消", "", true);
							return;
						}

						int nodeProxyPort = localPort + originalIndex;
						WebProxy nodeProxy = new WebProxy("127.0.0.1", nodeProxyPort);
						_updateSpeedFunc?.Invoke(node, "正在下载...", "", false);
						RunSingleNodeDownload(originalIndex, node, speedTestUrl, nodeProxy, 1, maxTimeSec, ct);
					}
					finally
					{
						if (acquired)
						{
							semaphore.Release();
						}
					}
				}));
			}

			Task.WaitAll(nodeTasks.ToArray());
		}
		catch
		{
		}
		finally
		{
			_MihomoHandler?.ShowMsg(updateToTrayTooltip: false, ct.IsCancellationRequested ? "下载测试已停止" : "下载测试完成！");
			_onCompleted?.Invoke();
		}
	}

	private void RunSingleNodeDownload(int originalIndex, VmessItem node, string url, WebProxy webProxy, int threadCount, int maxTimeSec, CancellationToken outerCt)
	{
		long totalBytes = 0L;
		string lastError = "连接超时";
		using CancellationTokenSource nodeCts = CancellationTokenSource.CreateLinkedTokenSource(outerCt);
		nodeCts.CancelAfter(maxTimeSec * 1000);
		CancellationToken token = nodeCts.Token;

		Stopwatch swTotal = Stopwatch.StartNew();
		List<HttpWebRequest> activeRequests = new List<HttpWebRequest>();
		object reqLock = new object();

		using CancellationTokenRegistration reg = token.Register(() =>
		{
			lock (reqLock)
			{
				foreach (HttpWebRequest req in activeRequests)
				{
					try
					{
						req.Abort();
					}
					catch
					{
					}
				}
			}
		});

		List<Task> workerTasks = new List<Task>(threadCount);
		for (int t = 0; t < threadCount; t++)
		{
			int workerId = t;
			workerTasks.Add(Task.Run(() =>
			{
				string connGroup = $"dl_{originalIndex}_{workerId}_{Guid.NewGuid():N}";
				ServicePoint sp = null;
				try
				{
					while (!token.IsCancellationRequested)
					{
						HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
						req.Proxy = webProxy;
						req.Timeout = maxTimeSec * 1000;
						req.ReadWriteTimeout = maxTimeSec * 1000;
						req.KeepAlive = false;
						req.AllowReadStreamBuffering = false;
						req.ConnectionGroupName = connGroup;
						req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
						sp = req.ServicePoint;
						if (sp != null)
						{
							sp.ConnectionLimit = Math.Max(sp.ConnectionLimit, threadCount * 4);
						}
						lock (reqLock)
						{
							if (token.IsCancellationRequested)
							{
								return;
							}
							activeRequests.Add(req);
						}

						using HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
						using Stream stream = resp.GetResponseStream();
						if (stream == null)
						{
							break;
						}
						byte[] buffer = new byte[65536];
						int bytesRead;
						while (!token.IsCancellationRequested && (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
						{
							Interlocked.Add(ref totalBytes, bytesRead);
						}
					}
				}
				catch (WebException wex)
				{
					if (!token.IsCancellationRequested)
					{
						lastError = wex.Status == WebExceptionStatus.Timeout ? "连接超时" : "连接失败";
					}
				}
				catch (Exception)
				{
					if (!token.IsCancellationRequested)
					{
						lastError = "下载失败";
					}
				}
				finally
				{
					try
					{
						sp?.CloseConnectionGroup(connGroup);
					}
					catch
					{
					}
				}
			}));
		}

		double maxMbps = 0.0;
		long lastSampleBytes = 0L;
		double lastSampleSec = 0.0;

		while (!token.IsCancellationRequested && ! workerTasks.All(t => t.IsCompleted))
		{
			Thread.Sleep(200);
			long currentBytes = Interlocked.Read(ref totalBytes);
			double elapsedSec = swTotal.Elapsed.TotalSeconds;
			if (elapsedSec <= 0.05)
			{
				continue;
			}

			double windowSec = elapsedSec - lastSampleSec;
			if (windowSec >= 0.5)
			{
				long deltaBytes = currentBytes - lastSampleBytes;
				if (deltaBytes > 0)
				{
					double instantMbps = (double)deltaBytes * 8.0 / (windowSec * 1048576.0);
					if (instantMbps > maxMbps)
					{
						maxMbps = instantMbps;
					}
				}
				lastSampleBytes = currentBytes;
				lastSampleSec = elapsedSec;
			}

			if (currentBytes > 0)
			{
				double avgMbps = (double)currentBytes * 8.0 / (elapsedSec * 1048576.0);
				if (avgMbps > maxMbps)
				{
					maxMbps = avgMbps;
				}
				_updateSpeedFunc?.Invoke(node, $"测速中 {avgMbps:F2} Mbps", $"测速中 {maxMbps:F2} Mbps", false);
			}
		}

		nodeCts.Cancel();
		try
		{
			Task.WaitAll(workerTasks.ToArray(), 1000);
		}
		catch
		{
		}
		swTotal.Stop();

		long finalBytes = Interlocked.Read(ref totalBytes);
		double totalSec = Math.Max(swTotal.Elapsed.TotalSeconds, 0.1);

		if (finalBytes > 0)
		{
			double finalAvgMbps = (double)finalBytes * 8.0 / (totalSec * 1048576.0);
			if (finalAvgMbps > maxMbps)
			{
				maxMbps = finalAvgMbps;
			}
			_updateSpeedFunc?.Invoke(node, $"{finalAvgMbps:F2} Mbps", $"{maxMbps:F2} Mbps", true);
		}
		else if (outerCt.IsCancellationRequested)
		{
			_updateSpeedFunc?.Invoke(node, "测速被取消", "", true);
		}
		else
		{
			_updateSpeedFunc?.Invoke(node, lastError, "", true);
		}
	}

	private static string GetRealPingTime(string url, int timeOut)
	{
		try
		{
			HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(url);
			obj.Timeout = timeOut;
			obj.ReadWriteTimeout = timeOut;
			obj.KeepAlive = true;
			using HttpWebResponse httpWebResponse = (HttpWebResponse)obj.GetResponse();
			using Stream stream = httpWebResponse.GetResponseStream();
			using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
			return streamReader.ReadToEnd();
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	private static bool TryParseDelayMs(string responseText, out int delayMs, out string errorMsg)
	{
		delayMs = -1;
		errorMsg = "超时";
		if (string.IsNullOrEmpty(responseText))
		{
			return false;
		}
		Match match = Regex.Match(responseText, "\"delay\"\\s*:\\s*([0-9]+)");
		if (match.Success && int.TryParse(match.Groups[1].Value, out delayMs) && delayMs > 0)
		{
			errorMsg = string.Empty;
			return true;
		}
		if (responseText.IndexOf("504") != -1 || responseText.IndexOf("超时") != -1 || responseText.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) != -1)
		{
			errorMsg = "超时";
		}
		else if (responseText.IndexOf("503") != -1 || responseText.IndexOf("502") != -1)
		{
			errorMsg = "无法连接";
		}
		else
		{
			errorMsg = "测试失败";
		}
		return false;
	}

	private int GetCurrentNodeIndex(int originalIndex)
	{
		if (!_nodeSnapshot.ContainsKey(originalIndex))
		{
			return -1;
		}
		VmessItem vmessItem = _nodeSnapshot[originalIndex];
		for (int i = 0; i < _config.vmess.Count; i++)
		{
			if (_config.vmess[i] == vmessItem)
			{
				return i;
			}
		}
		return -1;
	}
}
