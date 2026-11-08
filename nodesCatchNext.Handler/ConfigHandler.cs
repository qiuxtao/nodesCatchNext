using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using nodesCatchNext.Base;
using nodesCatchNext.Mode;

namespace nodesCatchNext.Handler;

internal class ConfigHandler
{
	private static string configRes = "nodeConfig.json";

	public static event ProcessDelegate ProcessEvent;

	public static int LoadConfig(ref Config config)
	{
		string text = Utils.LoadResource(Utils.GetPath(configRes));
		if (!Utils.IsNullOrEmpty(text))
		{
			config = Utils.FromJson<Config>(text);
			ShowMsg(b: false, "配置文件加载成功！");
		}
		if (config == null)
		{
			ShowMsg(b: false, "未找到配置文件，已生成默认配置文件");
			config = new Config
			{
				vmess = new List<VmessItem>()
			};
		}
		if (config.subItem == null)
		{
			config.subItem = new List<SubItem>();
		}
		foreach (SubItem item in config.subItem)
		{
			if (Utils.IsNullOrEmpty(item.id))
			{
				item.id = Utils.GetGUID();
			}
		}
		if (config.localPort == 0)
		{
			config.localPort = 40000;
		}
		if (config.externalControllerPort == 0)
		{
			config.externalControllerPort = 40001;
		}
		if (config.externalController == null)
		{
			config.externalController = "127.0.0.1:40001";
		}
		if (config.uiItem == null)
		{
			config.uiItem = new UIItem
			{
				mainSize = new Size(1640, 900)
			};
		}
		if (config.uiItem.mainLvColWidth == null)
		{
			config.uiItem.mainLvColWidth = new Dictionary<string, int>
			{
				{ "def", 40 },
				{ "configType", 80 },
				{ "remarks", 200 },
				{ "address", 120 },
				{ "port", 50 },
				{ "security", 90 },
				{ "network", 70 },
				{ "tls", 80 },
				{ "subRemarks", 70 },
				{ "minDelay", 85 },
				{ "avgDelay", 85 },
				{ "testResult", 120 },
				{ "MaxSpeed", 120 },
				{ "lastTestTime", 130 }
			};
		}
		else
		{
			if (!config.uiItem.mainLvColWidth.ContainsKey("minDelay"))
			{
				config.uiItem.mainLvColWidth.Add("minDelay", 85);
			}
			if (!config.uiItem.mainLvColWidth.ContainsKey("avgDelay"))
			{
				config.uiItem.mainLvColWidth.Add("avgDelay", 85);
			}
			if (!config.uiItem.mainLvColWidth.ContainsKey("lastTestTime"))
			{
				config.uiItem.mainLvColWidth.Add("lastTestTime", 130);
			}
		}
		// 初始化列可见性配置
		if (config.uiItem.mainLvColVisible == null)
		{
			config.uiItem.mainLvColVisible = new Dictionary<string, bool>
			{
				{ "def", true },
				{ "configType", true },
				{ "remarks", true },
				{ "address", true },
				{ "port", true },
				{ "security", true },
				{ "network", true },
				{ "tls", true },
				{ "subRemarks", true },
				{ "minDelay", true },
				{ "avgDelay", true },
				{ "testResult", true },
				{ "MaxSpeed", true },
				{ "lastTestTime", true }
			};
		}
		else
		{
			if (!config.uiItem.mainLvColVisible.ContainsKey("minDelay"))
			{
				config.uiItem.mainLvColVisible["minDelay"] = true;
			}
			if (!config.uiItem.mainLvColVisible.ContainsKey("avgDelay"))
			{
				config.uiItem.mainLvColVisible["avgDelay"] = true;
			}
			if (!config.uiItem.mainLvColVisible.ContainsKey("lastTestTime"))
			{
				config.uiItem.mainLvColVisible["lastTestTime"] = true;
			}
		}
		if (Utils.IsNullOrEmpty(config.speedTestUrl))
		{
			config.speedTestUrl = "https://cdn.kernel.org/pub/linux/kernel/v6.x/linux-6.2.15.tar.xz";
		}
		if (Utils.IsNullOrEmpty(config.speedPingTestUrl))
		{
			config.speedPingTestUrl = "https://www.cloudflare.com/cdn-cgi/trace";
		}
		if (Utils.IsNullOrEmpty(config.PingCount))
		{
			config.PingCount = "3";
		}
		if (Utils.IsNullOrEmpty(config.Timeout))
		{
			config.Timeout = "5";
		}
		if (Utils.IsNullOrEmpty(config.DownloadThread))
		{
			config.DownloadThread = "4";
		}
		if (Utils.IsNullOrEmpty(config.DownloadMaxTime))
		{
			config.DownloadMaxTime = "10";
		}
		if (config.vmess != null)
		{
			foreach (VmessItem item2 in config.vmess)
			{
				if (string.IsNullOrEmpty(item2.minDelay) && !string.IsNullOrEmpty(item2.httpsDelay))
				{
					item2.minDelay = item2.httpsDelay;
				}
				if (string.IsNullOrEmpty(item2.avgDelay) && !string.IsNullOrEmpty(item2.httpsDelay))
				{
					item2.avgDelay = item2.httpsDelay;
				}
				item2.testResult = NormalizeSpeedToMbps(item2.testResult);
				item2.MaxSpeed = NormalizeSpeedToMbps(item2.MaxSpeed);
			}
		}
		config.defAllowInsecure = true;
		return 0;
	}

	private static string NormalizeSpeedToMbps(string speedText)
	{
		if (string.IsNullOrEmpty(speedText))
		{
			return speedText;
		}
		string trimmed = speedText.Trim();
		if (trimmed.EndsWith("MB/s", StringComparison.OrdinalIgnoreCase))
		{
			string numPart = trimmed.Substring(0, trimmed.Length - 4).Trim();
			if (double.TryParse(numPart, out var mbpsVal))
			{
				return $"{mbpsVal * 8.0:F2} Mbps";
			}
		}
		else if (trimmed.EndsWith("KB/s", StringComparison.OrdinalIgnoreCase))
		{
			string numPart = trimmed.Substring(0, trimmed.Length - 4).Trim();
			if (double.TryParse(numPart, out var kbpsVal))
			{
				return $"{kbpsVal * 8.0 / 1024.0:F2} Mbps";
			}
		}
		return speedText;
	}

	public static void ShowMsg(bool b, string msg)
	{
		ConfigHandler.ProcessEvent?.Invoke(b, msg);
	}

	public static int AddformMainLvColWidth(ref Config config, string name, int width)
	{
		if (config.uiItem.mainLvColWidth == null)
		{
			config.uiItem.mainLvColWidth = new Dictionary<string, int>();
		}
		if (config.uiItem.mainLvColWidth.ContainsKey(name))
		{
			config.uiItem.mainLvColWidth[name] = width;
		}
		else
		{
			config.uiItem.mainLvColWidth.Add(name, width);
		}
		return 0;
	}

	public static int GetformMainLvColWidth(ref Config config, string name, int width)
	{
		if (config.uiItem.mainLvColWidth == null)
		{
			config.uiItem.mainLvColWidth = new Dictionary<string, int>();
		}
		if (config.uiItem.mainLvColWidth.ContainsKey(name))
		{
			return config.uiItem.mainLvColWidth[name];
		}
		return width;
	}

	public static int SaveConfig(ref Config config, bool reload)
	{
		
		ToJsonFile(config);
		return 0;
	}

	public static int SaveUIConfigOnly(Config currentConfig)
	{
		try
		{
			ToJsonFile(currentConfig);
			return 0;
		}
		catch
		{
			return -1;
		}
	}

	public static void ToJsonFile(Config config)
	{
		config.index = 0;
		Utils.ToJsonFile(config, Utils.GetPath(configRes));
	}

	public static int AddBatchServers(ref Config config, string clipboardData, string subid = "")
	{
		if (Utils.IsNullOrEmpty(clipboardData))
		{
			return -1;
		}
		if (!string.IsNullOrEmpty(subid))
		{
			config.vmess?.RemoveAll((VmessItem v) => v.subid == subid);
		}
		int num = 0;
		string[] array = clipboardData.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			string text = array[num2].Trim();
			if (Utils.IsNullOrEmpty(text))
			{
				continue;
			}
			string msg;
			VmessItem vmessItem = ShareHandler.ImportFromClipboardConfig(text, out msg);
			if (vmessItem == null)
			{
				continue;
			}
			vmessItem.subid = subid;
			if (vmessItem.configType == 1)
			{
				if (AddServer(ref config, vmessItem, -1) == 0)
				{
					num++;
				}
			}
			else if (vmessItem.configType == 3)
			{
				if (AddShadowsocksServer(ref config, vmessItem, -1) == 0)
				{
					num++;
				}
			}
			else if (vmessItem.configType == 4)
			{
				if (AddSocksServer(ref config, vmessItem, -1) == 0)
				{
					num++;
				}
			}
			else if (vmessItem.configType == 7)
			{
				if (AddHttpServer(ref config, vmessItem, -1) == 0)
				{
					num++;
				}
			}
			else if (vmessItem.configType == 8)
			{
				if (AddHttpsServer(ref config, vmessItem, -1) == 0)
				{
					num++;
				}
			}
			else if (vmessItem.configType == 6)
			{
				if (AddTrojanServer(ref config, vmessItem, -1) == 0)
				{
					num++;
				}
			}
			else if (vmessItem.configType == 5)
			{
				if (AddVlessServer(ref config, vmessItem, -1) == 0)
				{
					num++;
				}
			}
			else if (vmessItem.configType == 9)
			{
				if (AddSSRServer(ref config, vmessItem, -1) == 0)
				{
					num++;
				}
			}
			else if (vmessItem.configType == 11 && AddHysteria2Server(ref config, vmessItem, -1) == 0)
			{
				num++;
			}
			else if (vmessItem.configType == 12 && AddAnyTLSServer(ref config, vmessItem, -1) == 0)
			{
				num++;
			}
		}
		return num;
	}

		public static int AddSSRServer(ref Config config, VmessItem vmessItem, int index)
		{
			vmessItem.configVersion = 2;
			vmessItem.configType = 9;
			vmessItem.address = vmessItem.address.TrimEx();
			vmessItem.id = vmessItem.id.TrimEx();
			vmessItem.security = vmessItem.security.TrimEx();
			vmessItem.network = vmessItem.network.TrimEx();
			vmessItem.headerType = vmessItem.headerType.TrimEx();
			// 设置默认值
			if (Utils.IsNullOrEmpty(vmessItem.network))
			{
				vmessItem.network = "origin";
			}
			if (Utils.IsNullOrEmpty(vmessItem.headerType))
			{
				vmessItem.headerType = "plain";
			}
			if (index >= 0)
			{
				config.vmess[index] = vmessItem;
				if (config.index.Equals(index))
				{
					
				}
			}
			else
			{
				config.vmess.Add(vmessItem);
				if (config.vmess.Count == 1)
				{
					config.index = 0;
					
				}
			}
			return 0;
		}

		public static int AddVlessServer(ref Config config, VmessItem vmessItem, int index)
		{
			vmessItem.configVersion = 2;
			vmessItem.configType = 5;
			vmessItem.address = vmessItem.address.TrimEx();
			vmessItem.id = vmessItem.id.TrimEx();
			vmessItem.security = vmessItem.security.TrimEx();
			vmessItem.network = vmessItem.network.TrimEx();
			vmessItem.headerType = vmessItem.headerType.TrimEx();
			vmessItem.requestHost = vmessItem.requestHost.TrimEx();
			vmessItem.path = vmessItem.path.TrimEx();
			vmessItem.streamSecurity = vmessItem.streamSecurity.TrimEx();
			if (index >= 0)
			{
				config.vmess[index] = vmessItem;
				if (config.index.Equals(index))
				{
					
				}
			}
			else
			{
				if (Utils.IsNullOrEmpty(vmessItem.allowInsecure))
				{
					vmessItem.allowInsecure = config.defAllowInsecure.ToString();
				}
				config.vmess.Add(vmessItem);
				if (config.vmess.Count == 1)
				{
					config.index = 0;
					
				}
			}
			return 0;
		}

		public static int AddShadowsocksServer(ref Config config, VmessItem vmessItem, int index)
	{
		vmessItem.configVersion = 2;
		vmessItem.configType = 3;
		vmessItem.address = vmessItem.address.TrimEx();
		vmessItem.id = vmessItem.id.TrimEx();
		vmessItem.security = vmessItem.security.TrimEx();
		vmessItem.network = vmessItem.network.TrimEx();
		if (!Global.ssSecuritys.Contains(vmessItem.security))
		{
			return -1;
		}
		if (index >= 0)
		{
			config.vmess[index] = vmessItem;
			if (config.index.Equals(index))
			{
				
			}
		}
		else
		{
			config.vmess.Add(vmessItem);
			if (config.vmess.Count == 1)
			{
				config.index = 0;
				
			}
		}
		return 0;
	}

	public static int AddSocksServer(ref Config config, VmessItem vmessItem, int index)
	{
		vmessItem.configVersion = 2;
		vmessItem.configType = 4;
		vmessItem.address = vmessItem.address.TrimEx();
		if (index >= 0)
		{
			config.vmess[index] = vmessItem;
			if (config.index.Equals(index))
			{
				
			}
		}
		else
		{
			config.vmess.Add(vmessItem);
			if (config.vmess.Count == 1)
			{
				config.index = 0;
				
			}
		}
		return 0;
	}

	public static int AddHttpServer(ref Config config, VmessItem vmessItem, int index)
	{
		vmessItem.configVersion = 2;
		vmessItem.configType = 7;
		vmessItem.address = vmessItem.address.TrimEx();
		if (index >= 0)
		{
			config.vmess[index] = vmessItem;
			if (config.index.Equals(index))
			{
				
			}
		}
		else
		{
			config.vmess.Add(vmessItem);
			if (config.vmess.Count == 1)
			{
				config.index = 0;
				
			}
		}
		return 0;
	}

	public static int AddHttpsServer(ref Config config, VmessItem vmessItem, int index)
	{
		vmessItem.configVersion = 2;
		vmessItem.configType = 8;
		vmessItem.address = vmessItem.address.TrimEx();
		if (index >= 0)
		{
			config.vmess[index] = vmessItem;
			if (config.index.Equals(index))
			{
				
			}
		}
		else
		{
			config.vmess.Add(vmessItem);
			if (config.vmess.Count == 1)
			{
				config.index = 0;
				
			}
		}
		return 0;
	}

	public static int AddTrojanServer(ref Config config, VmessItem vmessItem, int index)
	{
		vmessItem.configVersion = 2;
		vmessItem.configType = 6;
		vmessItem.address = vmessItem.address.TrimEx();
		vmessItem.id = vmessItem.id.TrimEx();
		vmessItem.streamSecurity = "tls";
		if (Utils.IsNullOrEmpty(vmessItem.allowInsecure))
		{
			vmessItem.allowInsecure = config.defAllowInsecure.ToString();
		}
		if (index >= 0)
		{
			config.vmess[index] = vmessItem;
			if (config.index.Equals(index))
			{
				
			}
		}
		else
		{
			config.vmess.Add(vmessItem);
			if (config.vmess.Count == 1)
			{
				config.index = 0;
				
			}
		}
		return 0;
	}

	public static int AddHysteria2Server(ref Config config, VmessItem vmessItem, int index)
	{
		vmessItem.configVersion = 2;
		vmessItem.configType = 11;
		vmessItem.address = vmessItem.address.TrimEx();
		vmessItem.id = vmessItem.id.TrimEx();
		vmessItem.network = "hysteria2";
		vmessItem.streamSecurity = "tls";
		vmessItem.sni = vmessItem.sni.TrimEx();
		vmessItem.path = vmessItem.path.TrimEx();
		vmessItem.requestHost = vmessItem.requestHost.TrimEx();
		vmessItem.publicKey = vmessItem.publicKey.TrimEx();
		if (Utils.IsNullOrEmpty(vmessItem.allowInsecure))
		{
			vmessItem.allowInsecure = config.defAllowInsecure.ToString();
		}
		if (index >= 0)
		{
			config.vmess[index] = vmessItem;
			if (config.index.Equals(index))
			{
				
			}
		}
		else
		{
			config.vmess.Add(vmessItem);
			if (config.vmess.Count == 1)
			{
				config.index = 0;
				
			}
		}
		return 0;
	}

	public static int AddAnyTLSServer(ref Config config, VmessItem vmessItem, int index)
	{
		vmessItem.configVersion = 2;
		vmessItem.configType = 12;
		vmessItem.address = vmessItem.address.TrimEx();
		vmessItem.id = vmessItem.id.TrimEx();
		vmessItem.network = "anytls";
		vmessItem.streamSecurity = "tls";
		vmessItem.sni = vmessItem.sni.TrimEx();
		if (Utils.IsNullOrEmpty(vmessItem.allowInsecure))
		{
			vmessItem.allowInsecure = config.defAllowInsecure.ToString();
		}
		if (index >= 0)
		{
			config.vmess[index] = vmessItem;
			if (config.index.Equals(index))
			{
				
			}
		}
		else
		{
			config.vmess.Add(vmessItem);
			if (config.vmess.Count == 1)
			{
				config.index = 0;
				
			}
		}
		return 0;
	}

	public static int AddServer(ref Config config, VmessItem vmessItem, int index)
	{
		vmessItem.configVersion = 2;
		vmessItem.configType = 1;
		vmessItem.address = vmessItem.address.TrimEx();
		vmessItem.id = vmessItem.id.TrimEx();
		vmessItem.security = vmessItem.security.TrimEx();
		vmessItem.network = vmessItem.network.TrimEx();
		vmessItem.headerType = vmessItem.headerType.TrimEx();
		vmessItem.requestHost = vmessItem.requestHost.TrimEx();
		vmessItem.path = vmessItem.path.TrimEx();
		vmessItem.streamSecurity = vmessItem.streamSecurity.TrimEx();
		if (index >= 0)
		{
			config.vmess[index] = vmessItem;
			if (config.index.Equals(index))
			{
				
			}
		}
		else
		{
			if (Utils.IsNullOrEmpty(vmessItem.allowInsecure))
			{
				vmessItem.allowInsecure = config.defAllowInsecure.ToString();
			}
			config.vmess.Add(vmessItem);
		}
		return 0;
	}

	public static int RemoveServer(ref Config config, int index)
	{
		if (index < 0 || index > config.vmess.Count - 1)
		{
			return -1;
		}
		config.vmess.RemoveAt(index);
		return 0;
	}

	public static int AddSubItem(ref Config config, string url)
	{
		foreach (SubItem item2 in config.subItem)
		{
			if (url == item2.url)
			{
				return 0;
			}
		}
		SubItem item = new SubItem
		{
			id = Utils.GetGUID(),
			remarks = "剪贴板导入",
			url = url
		};
		config.subItem.Add(item);
		return SaveSubItem(ref config);
	}

	public static int SaveSubItem(ref Config config)
	{
		if (config.subItem == null || config.subItem.Count <= 0)
		{
			return -1;
		}
		foreach (SubItem item in config.subItem)
		{
			if (Utils.IsNullOrEmpty(item.id))
			{
				item.id = Utils.GetGUID();
			}
		}
		return 0;
	}

	public static int SortServers(ref Config config, EServerColName name, bool asc)
	{
		if (config.vmess.Count <= 0)
		{
			return -1;
		}
		// 只允许排序特定列
		if (name == EServerColName.def)
		{
			return -1;
		}
		switch (name)
		{
		case EServerColName.MaxSpeed:
			if (asc)
			{
				config.vmess = (from item in config.vmess
					orderby HasTestResult(item.MaxSpeed) descending, HasTestResult(item.MaxSpeed) ? ParseMaxSpeed(item.MaxSpeed) : 0.0
					select item).ToList();
			}
			else
			{
				config.vmess = (from item in config.vmess
					orderby HasTestResult(item.MaxSpeed) descending, HasTestResult(item.MaxSpeed) ? ParseMaxSpeed(item.MaxSpeed) : 0.0 descending
					select item).ToList();
			}
			break;
		case EServerColName.tls:
			if (asc)
			{
				config.vmess = config.vmess.OrderBy((VmessItem item) => Utils.IsNullOrEmpty(item.streamSecurity) ? "none" : item.streamSecurity).ToList();
			}
			else
			{
				config.vmess = config.vmess.OrderByDescending((VmessItem item) => Utils.IsNullOrEmpty(item.streamSecurity) ? "none" : item.streamSecurity).ToList();
			}
			break;
		case EServerColName.subRemarks:
		{
			Config configCopy = config;
			if (asc)
			{
				config.vmess = config.vmess.OrderBy((VmessItem item) => item.getSubRemarks(configCopy)).ToList();
			}
			else
			{
				config.vmess = config.vmess.OrderByDescending((VmessItem item) => item.getSubRemarks(configCopy)).ToList();
			}
			break;
		}
		case EServerColName.minDelay:
			if (asc)
			{
				config.vmess = (from item in config.vmess
					orderby HasTestResult(item.minDelay) descending, HasTestResult(item.minDelay) ? ParseHttpsDelay(item.minDelay) : 0.0
					select item).ToList();
			}
			else
			{
				config.vmess = (from item in config.vmess
					orderby HasTestResult(item.minDelay) descending, HasTestResult(item.minDelay) ? ParseHttpsDelay(item.minDelay) : 0.0 descending
					select item).ToList();
			}
			break;
		case EServerColName.avgDelay:
			if (asc)
			{
				config.vmess = (from item in config.vmess
					orderby HasTestResult(item.avgDelay) descending, HasTestResult(item.avgDelay) ? ParseHttpsDelay(item.avgDelay) : 0.0
					select item).ToList();
			}
			else
			{
				config.vmess = (from item in config.vmess
					orderby HasTestResult(item.avgDelay) descending, HasTestResult(item.avgDelay) ? ParseHttpsDelay(item.avgDelay) : 0.0 descending
					select item).ToList();
			}
			break;
		case EServerColName.testResult:
			if (asc)
			{
				config.vmess = (from item in config.vmess
					orderby HasTestResult(item.testResult) descending, HasTestResult(item.testResult) ? ParseTestResult(item.testResult) : 0.0
					select item).ToList();
			}
			else
			{
				config.vmess = (from item in config.vmess
					orderby HasTestResult(item.testResult) descending, HasTestResult(item.testResult) ? ParseTestResult(item.testResult) : 0.0 descending
					select item).ToList();
			}
			break;
		case EServerColName.lastTestTime:
			if (asc)
			{
				config.vmess = (from item in config.vmess
					orderby !string.IsNullOrEmpty(item.lastTestTime) descending, item.lastTestTime
					select item).ToList();
			}
			else
			{
				config.vmess = (from item in config.vmess
					orderby !string.IsNullOrEmpty(item.lastTestTime) descending, item.lastTestTime descending
					select item).ToList();
			}
			break;
		default:
		{
			IQueryable<VmessItem> query = config.vmess.AsQueryable();
			if (asc)
			{
				config.vmess = query.OrderBy(name.ToString()).ToList();
			}
			else
			{
				config.vmess = query.OrderByDescending(name.ToString()).ToList();
			}
			break;
		}
		}
		return 0;
	}

	private static double ParseHttpsDelay(string httpsDelay)
	{
		if (string.IsNullOrEmpty(httpsDelay))
		{
			return -1.0;
		}
		if (httpsDelay.Contains("超时") || httpsDelay.Contains("Timeout") || httpsDelay.Contains("无法连接"))
		{
			return double.MaxValue;
		}
		if (httpsDelay.Contains("ms") && double.TryParse(httpsDelay.Replace("ms", "").Trim(), out var result))
		{
			return result;
		}
		return -1.0;
	}

	private static double ParseTestResult(string testResult)
	{
		if (string.IsNullOrEmpty(testResult))
		{
			return -1.0;
		}
		if (testResult.Contains("超时") || testResult.Contains("Timeout") || testResult.Contains("无法连接") || testResult.Contains("测速被取消") || testResult.Contains("等待测速") || testResult.Contains("正在"))
		{
			return -1.0;
		}
		testResult = testResult.Trim();
		if (testResult.IndexOf("Mbps", StringComparison.OrdinalIgnoreCase) != -1)
		{
			int idx = testResult.IndexOf("Mbps", StringComparison.OrdinalIgnoreCase);
			if (double.TryParse(testResult.Substring(0, idx).Trim(), out var mbps))
			{
				return mbps;
			}
		}
		if (testResult.IndexOf("MB/s", StringComparison.OrdinalIgnoreCase) != -1)
		{
			int idx = testResult.IndexOf("MB/s", StringComparison.OrdinalIgnoreCase);
			if (double.TryParse(testResult.Substring(0, idx).Trim(), out var mbs))
			{
				return mbs * 8.0;
			}
		}
		if (testResult.IndexOf("KB/s", StringComparison.OrdinalIgnoreCase) != -1)
		{
			int idx = testResult.IndexOf("KB/s", StringComparison.OrdinalIgnoreCase);
			if (double.TryParse(testResult.Substring(0, idx).Trim(), out var kbs))
			{
				return kbs * 8.0 / 1024.0;
			}
		}
		if (double.TryParse(testResult, out var raw))
		{
			return raw;
		}
		return -1.0;
	}

	private static double ParseMaxSpeed(string maxSpeed)
	{
		return ParseTestResult(maxSpeed);
	}

	private static bool HasTestResult(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		if (value.Contains("超时") || value.Contains("Timeout") || value.Contains("无法连接") || value.Contains("失败") || value.Contains("取消") || value.Contains("等待") || value.Contains("正在") || value.Contains("测试中") || value.Contains("测速中"))
		{
			return false;
		}
		return value.Contains("ms") || value.IndexOf("Mbps", StringComparison.OrdinalIgnoreCase) >= 0 || value.IndexOf("MB/s", StringComparison.OrdinalIgnoreCase) >= 0 || value.IndexOf("KB/s", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public static string sendReq(string args, string uri, string method)
	{
		string result = "";
		try
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			httpWebRequest.Method = method;
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Timeout = 300;
			httpWebRequest.ReadWriteTimeout = 300;
			httpWebRequest.ContinueTimeout = 300;
			byte[] bytes = Encoding.UTF8.GetBytes(args);
			httpWebRequest.ContentLength = bytes.Length;
			using (Stream stream = httpWebRequest.GetRequestStream())
			{
				stream.Write(bytes, 0, bytes.Length);
			}
			using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
			{
				result = Convert.ToInt32(httpWebResponse.StatusCode).ToString();
			}
			return result;
		}
		catch (WebException ex)
		{
			if (ex.Response is HttpWebResponse httpWebResponse2)
			{
				result = Convert.ToInt32(httpWebResponse2.StatusCode).ToString();
				try
				{
					using StreamReader streamReader = new StreamReader(httpWebResponse2.GetResponseStream());
					string text = streamReader.ReadToEnd();
					if (!string.IsNullOrEmpty(text))
					{
						result = result + ": " + text;
					}
				}
				catch
				{
				}
			}
			else
			{
				result = ex.Message;
			}
			return result;
		}
		catch (Exception ex2)
		{
			return ex2.Message;
		}
	}

	public static bool TcpClientCheck(string ip, int port)
	{
		IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);
		TcpClient tcpClient = null;
		try
		{
			tcpClient = new TcpClient();
			tcpClient.Connect(remoteEP);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
		finally
		{
			tcpClient?.Close();
		}
	}
}
