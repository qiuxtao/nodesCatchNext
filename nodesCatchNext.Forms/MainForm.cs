using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using nodesCatchNext.Base;
using nodesCatchNext.Handler;
using nodesCatchNext.Mode;

namespace nodesCatchNext.Forms;

public class MainForm : Form
{
	private delegate void AppendTextDelegate(string text);

	private struct HDITEM
	{
		public int mask;

		public int cxy;

		public IntPtr pszText;

		public IntPtr hbm;

		public int cchTextMax;

		public int fmt;

		public IntPtr lParam;

		public int iImage;

		public int iOrder;

		public uint type;

		public IntPtr pvFilter;

		public uint state;
	}

	private const uint HDM_FIRST = 4608u;

	private const uint HDM_GETITEM = 4619u;

	private const uint HDM_SETITEM = 4620u;

	private const uint LVM_GETHEADER = 4127u;

	private const int HDF_SORTUP = 1024;

	private const int HDF_SORTDOWN = 512;

	private const int HDI_FORMAT = 4;

	private bool currentSortAscending = true;

	public static Config config;

	public MihomoHandler mihomoHandler;

	public SubconverHandler subconverHandler;

	private List<int> lvSelecteds = new List<int>();

	private Dictionary<int, VmessItem> speedtestNodeMap = new Dictionary<int, VmessItem>();

	public CancellationTokenSource cts;

	private IContainer components;

	private SplitContainer splitContainer1;

	private ListViewFlickerFree lvServers;

	private Panel panel1;

	private ContextMenuStrip cmsMain;

	private ToolStripMenuItem menuExit;

	private ContextMenuStrip cmsLv;

	private ToolStripMenuItem menuRealPingServer;

	private ToolStripMenuItem menuDownLoadServer;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripMenuItem menuAddServers;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripMenuItem menuSelectAll;

	private ToolStripMenuItem menuRemoveServer;

	private ToolStripMenuItem menuEditServer;

	private ToolStripMenuItem menuRemoveDuplicateServer;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripMenuItem menuExport2ShareUrl;

	private ToolStripMenuItem menuExport2SubContent;

	private ToolStripSeparator toolStripSeparator6;

	private ToolStripMenuItem menuExport2Base64;

	private ToolStripMenuItem menuExport2Clash;

	private GroupBox groupBox3;

	private Panel panel2;

	private GroupBox groupBox1;

	private TextBox txtMsgBox;

	private Button btnClearLog;

	private Button btnClearResult;

	private Label lblDelayUrl;

	private TextBox tbDelayUrl;

	private Label lblPingCount;

	private TextBox tbPingCount;

	private TextBox tbTimeout;

	private Label label4;

	private Button btnTestDelay;

	private Label lblSpeedUrl;

	private TextBox tbSpeedUrl;

	private Label lblDownloadThread;

	private TextBox tbDownloadThread;

	private Label lblDownloadMaxTime;

	private TextBox tbDownloadMaxTime;

	private Button btnTestSpeed;

	private bool isTestingDelay = false;

	private bool isTestingSpeed = false;

	private Button button2;

	private Button button1;

	private ContextMenuStrip contextMenuStrip1;

	private ToolStripMenuItem 订阅列表ToolStripMenuItem;

	private ToolStripMenuItem 导入到节点列表ToolStripMenuItem;

	private Panel groupBox5;

	private TabControl tabSubscriptions;

	private ContextMenuStrip cmsTabSubscriptions;

	private string rightClickedTabSubId;

	private int draggedTabIndex = -1;

	private Rectangle dragBoxFromMouseDown;

	private string currentSubFilter;

	private CheckBox cbKeywordFilter;

	private TextBox tbKeywordFilter;

	private Button btnKeywordPreset;

	private ContextMenuStrip cmsKeywordPreset;

	private ToolTip toolTipKeywordFilter;

	// 列头右键菜单
	private ContextMenuStrip cmsColumnHeader;

	private const int SubconverterPort = 25500;

	public static MainForm Instance { get; private set; }

	[DllImport("user32.dll")]
	private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", EntryPoint = "SendMessage")]
	private static extern IntPtr SendMessageHDITEM(IntPtr hWnd, uint Msg, IntPtr wParam, ref HDITEM lParam);

	public MainForm()
	{
		Instance = this;
		InitializeComponent();
		InitializeKeywordPresetMenu();
		InitializeSubscriptionTabControl();
		InitializeColumnHeaderMenu();
		Global.processJob = new Job();
		Application.ApplicationExit += delegate
		{
			MyAppExit(blWindowsShutDown: false);
		};
	}

	private void InitializeKeywordPresetMenu()
	{
		cmsKeywordPreset.Items.AddRange(new ToolStripItem[14]
		{
			CreatePresetMenuItem("\ud83c\udded\ud83c\uddf0 香港", "\ud83c\udded\ud83c\uddf0,香港,HK"),
			CreatePresetMenuItem("\ud83c\uddf9\ud83c\uddfc 台湾", "\ud83c\uddf9\ud83c\uddfc,台湾,TW"),
			CreatePresetMenuItem("\ud83c\uddef\ud83c\uddf5 日本", "\ud83c\uddef\ud83c\uddf5,日本,JP"),
			CreatePresetMenuItem("\ud83c\uddfa\ud83c\uddf8 美国", "\ud83c\uddfa\ud83c\uddf8,美国,US,USA"),
			CreatePresetMenuItem("\ud83c\uddf8\ud83c\uddec 新加坡", "\ud83c\uddf8\ud83c\uddec,新加坡,SG,SGP"),
			CreatePresetMenuItem("\ud83c\uddf0\ud83c\uddf7 韩国", "\ud83c\uddf0\ud83c\uddf7,韩国,KR"),
			new ToolStripSeparator(),
			CreatePresetMenuItem("\ud83c\uddec\ud83c\udde7 英国", "\ud83c\uddec\ud83c\udde7,英国,UK,GB"),
			CreatePresetMenuItem("\ud83c\udde9\ud83c\uddea 德国", "\ud83c\udde9\ud83c\uddea,德国,DE"),
			CreatePresetMenuItem("\ud83c\udde8\ud83c\udde6 加拿大", "\ud83c\udde8\ud83c\udde6,加拿大,CA"),
			CreatePresetMenuItem("\ud83c\udde6\ud83c\uddfa 澳大利亚", "\ud83c\udde6\ud83c\uddfa,澳大利亚,AU"),
			CreatePresetMenuItem("\ud83c\uddeb\ud83c\uddf7 法国", "\ud83c\uddeb\ud83c\uddf7,法国,FR"),
			new ToolStripSeparator(),
			CreatePresetMenuItem("\ud83c\udf10 全球/清空", "", "CLEAR")
		});
	}

	// 列头 NativeWindow 实例，用于重新绑定
	private ColumnHeaderNativeWindow columnHeaderNativeWindow;

	private void InitializeColumnHeaderMenu()
	{
		cmsColumnHeader = new ContextMenuStrip();
		// 绑定会在 InitServersView 末尾调用 AttachColumnHeaderMenu 完成
	}

	private void AttachColumnHeaderMenu()
	{
		// 释放旧的 NativeWindow
		if (columnHeaderNativeWindow != null)
		{
			columnHeaderNativeWindow.ReleaseHandle();
			columnHeaderNativeWindow = null;
		}
		
		// 获取列头控件句柄并设置右键菜单
		IntPtr headerHandle = SendMessage(lvServers.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
		if (headerHandle != IntPtr.Zero)
		{
			columnHeaderNativeWindow = new ColumnHeaderNativeWindow(this, cmsColumnHeader);
			columnHeaderNativeWindow.AssignHandle(headerHandle);
		}
	}

	// 内部类：用于拦截列头窗口消息
	private class ColumnHeaderNativeWindow : NativeWindow
	{
		private const int WM_CONTEXTMENU = 0x007B;
		private readonly MainForm _mainForm;
		private readonly ContextMenuStrip _columnMenu;

		public ColumnHeaderNativeWindow(MainForm mainForm, ContextMenuStrip columnMenu)
		{
			_mainForm = mainForm;
			_columnMenu = columnMenu;
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_CONTEXTMENU)
			{
				// 在列头上右键，显示列头菜单
				int x = (int)(m.LParam.ToInt64() & 0xFFFF);
				int y = (int)((m.LParam.ToInt64() >> 16) & 0xFFFF);
				
				// 直接构建并显示菜单
				_mainForm.BuildColumnHeaderMenu();
				_columnMenu.Show(x, y);
				return; // 阻止默认处理
			}
			base.WndProc(ref m);
		}
	}

	private void BuildColumnHeaderMenu()
	{
		cmsColumnHeader.Items.Clear();
		
		// 添加"将列调整为合适的大小"选项
		var menuAutoFitColumn = new ToolStripMenuItem("将列调整为合适的大小");
		menuAutoFitColumn.Click += (s, args) => AutoFitAllColumns();
		cmsColumnHeader.Items.Add(menuAutoFitColumn);
		
		cmsColumnHeader.Items.Add(new ToolStripSeparator());
		
		// 按当前列顺序添加所有列选项
		foreach (var colDef in columnDefinitions)
		{
			var menuItem = new ToolStripMenuItem(colDef.displayName);
			menuItem.Tag = colDef.key;
			menuItem.Checked = IsColumnVisible(colDef.key);
			menuItem.Click += ColumnHeaderMenuItem_Click;
			cmsColumnHeader.Items.Add(menuItem);
		}
	}

	private void ColumnHeaderMenuItem_Click(object sender, EventArgs e)
	{
		if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string columnKey)
		{
			// 切换列的可见性
			bool newVisible = !IsColumnVisible(columnKey);
			SetColumnVisible(columnKey, newVisible);
			
			// 保存配置并刷新列表
			SaveColumnVisibility();
			RebuildColumns();
		}
	}

	private void SetColumnVisible(string columnKey, bool visible)
	{
		if (config.uiItem.mainLvColVisible == null)
		{
			config.uiItem.mainLvColVisible = new Dictionary<string, bool>();
		}
		config.uiItem.mainLvColVisible[columnKey] = visible;
	}

	private void SaveColumnVisibility()
	{
		ConfigHandler.ToJsonFile(config);
	}

	private void RebuildColumns()
	{
		// 保存当前选中项
		var selectedIndices = new List<int>();
		foreach (int idx in lvServers.SelectedIndices)
		{
			if (lvServers.Items[idx].Tag is int vmessIdx)
				selectedIndices.Add(vmessIdx);
		}
		
		// 重建列
		lvServers.BeginUpdate();
		lvServers.Columns.Clear();
		lvServers.Items.Clear();
		
		// 获取列顺序
		currentColumnOrder = GetColumnOrder();
		
		// 按顺序添加列
		foreach (var key in currentColumnOrder)
		{
			var colDef = columnDefinitions.FirstOrDefault(c => c.key == key);
			if (colDef.key != null)
			{
				var col = lvServers.Columns.Add(colDef.displayName, GetVisibleColumnWidth(key, colDef.defaultWidth), HorizontalAlignment.Center);
				col.Tag = key;
			}
		}
		
		EnsureColumnHeaderMinWidths();
		lvServers.EndUpdate();
		
		// 刷新数据
		RefreshServersView();
		
		// 恢复选中项
		foreach (ListViewItem item in lvServers.Items)
		{
			if (item.Tag is int vmessIdx && selectedIndices.Contains(vmessIdx))
			{
				item.Selected = true;
			}
		}
		
		// 重新绑定列头右键菜单
		AttachColumnHeaderMenu();
	}

	private void AutoFitAllColumns()
	{
		lvServers.BeginUpdate();
		for (int i = 0; i < lvServers.Columns.Count; i++)
		{
			// 跳过隐藏列
			if (lvServers.Columns[i].Width == 0)
				continue;
			
			int autoWidth = CalculateAutoColumnWidth(i);
			if (autoWidth > 0)
			{
				lvServers.Columns[i].Width = autoWidth;
			}
		}
		lvServers.EndUpdate();
	}

	// 列定义：key, 显示名称, 默认宽度
	private readonly List<(string key, string displayName, int defaultWidth)> columnDefinitions = new List<(string, string, int)>
	{
		("def", "No", 40),
		("configType", "类型", 80),
		("remarks", "别名", 200),
		("address", "服务器地址", 120),
		("port", "端口", 50),
		("security", "加密方式", 90),
		("network", "传输协议", 70),
		("tls", "TLS", 80),
		("subRemarks", "订阅", 70),
		("minDelay", "最低延迟", 85),
		("avgDelay", "平均延迟", 85),
		("testResult", "平均速度", 120),
		("MaxSpeed", "峰值速度", 120),
		("lastTestTime", "最后测速", 130)
	};

	// 当前列顺序（key列表）
	private List<string> currentColumnOrder;

	private void InitServersView()
	{
		lvServers.BeginUpdate();
		lvServers.Items.Clear();
		lvServers.GridLines = true;
		lvServers.FullRowSelect = true;
		lvServers.View = View.Details;
		lvServers.Scrollable = true;
		lvServers.MultiSelect = true;
		lvServers.HeaderStyle = ColumnHeaderStyle.Clickable;
		lvServers.AllowColumnReorder = true;
		
		// 获取列顺序
		currentColumnOrder = GetColumnOrder();
		
		// 按顺序添加列
		foreach (var key in currentColumnOrder)
		{
			var colDef = columnDefinitions.FirstOrDefault(c => c.key == key);
			if (colDef.key != null)
			{
				var col = lvServers.Columns.Add(colDef.displayName, GetVisibleColumnWidth(key, colDef.defaultWidth), HorizontalAlignment.Center);
				col.Tag = key; // 保存列的 key 用于后续识别
			}
		}
		EnsureColumnHeaderMinWidths();
		lvServers.EndUpdate();
		
		// 绑定列顺序改变事件
		lvServers.ColumnReordered -= LvServers_ColumnReordered;
		lvServers.ColumnReordered += LvServers_ColumnReordered;
		
		// 绑定列头右键菜单
		AttachColumnHeaderMenu();
	}

	private void LvServers_ColumnReordered(object sender, ColumnReorderedEventArgs e)
	{
		// 延迟执行，等待列顺序实际更新完成
		BeginInvoke(new Action(() =>
		{
			SaveColumnOrder();
		}));
	}

	private void SaveColumnOrder()
	{
		if (config.uiItem.mainLvColOrder == null)
		{
			config.uiItem.mainLvColOrder = new List<string>();
		}
		
		config.uiItem.mainLvColOrder.Clear();
		
		// 按显示顺序保存列
		var displayOrder = new List<(int displayIndex, string key)>();
		for (int i = 0; i < lvServers.Columns.Count; i++)
		{
			var col = lvServers.Columns[i];
			if (col.Tag is string key)
			{
				displayOrder.Add((col.DisplayIndex, key));
			}
		}
		
		// 按 DisplayIndex 排序
		displayOrder.Sort((a, b) => a.displayIndex.CompareTo(b.displayIndex));
		
		foreach (var item in displayOrder)
		{
			config.uiItem.mainLvColOrder.Add(item.key);
		}
		
		// 更新当前列顺序
		currentColumnOrder = config.uiItem.mainLvColOrder.ToList();
		
		// 保存配置
		ConfigHandler.ToJsonFile(config);
	}

	private List<string> GetColumnOrder()
	{
		// 清理旧的 tlsRtt 和 httpsDelay 配置
		if (config.uiItem?.mainLvColOrder != null)
		{
			config.uiItem.mainLvColOrder.RemoveAll(k => k == "tlsRtt");
			int idx = config.uiItem.mainLvColOrder.IndexOf("httpsDelay");
			if (idx >= 0)
			{
				config.uiItem.mainLvColOrder.RemoveAt(idx);
				if (!config.uiItem.mainLvColOrder.Contains("avgDelay"))
					config.uiItem.mainLvColOrder.Insert(idx, "avgDelay");
				if (!config.uiItem.mainLvColOrder.Contains("minDelay"))
					config.uiItem.mainLvColOrder.Insert(idx, "minDelay");
			}
		}
		if (config.uiItem?.mainLvColVisible != null)
		{
			config.uiItem.mainLvColVisible.Remove("tlsRtt");
			config.uiItem.mainLvColVisible.Remove("httpsDelay");
		}
		if (config.uiItem?.mainLvColWidth != null)
		{
			config.uiItem.mainLvColWidth.Remove("tlsRtt");
			config.uiItem.mainLvColWidth.Remove("httpsDelay");
		}
		
		if (config.uiItem?.mainLvColOrder != null && config.uiItem.mainLvColOrder.Count > 0)
		{
			// 使用保存的顺序，但确保包含所有列
			var order = new List<string>(config.uiItem.mainLvColOrder);
			foreach (var col in columnDefinitions)
			{
				if (!order.Contains(col.key))
				{
					order.Add(col.key);
				}
			}
			return order;
		}
		// 默认顺序
		return columnDefinitions.Select(c => c.key).ToList();
	}

	private bool IsColumnVisible(string columnName)
	{
		if (config.uiItem?.mainLvColVisible == null)
			return true;
		if (config.uiItem.mainLvColVisible.TryGetValue(columnName, out bool visible))
			return visible;
		return true;
	}

	private int GetVisibleColumnWidth(string key, int defaultValue)
	{
		if (!IsColumnVisible(key))
			return 0;
		return GetColumnWidth(key, defaultValue);
	}

	/// <summary>
	/// 根据列名获取实际的列索引（考虑隐藏列）
	/// </summary>
	private int GetColumnIndexByName(string columnText)
	{
		for (int i = 0; i < lvServers.Columns.Count; i++)
		{
			if (lvServers.Columns[i].Text == columnText)
				return i;
		}
		return -1;
	}

	/// <summary>
	/// 根据列索引获取对应的 EServerColName 枚举值
	/// </summary>
	private EServerColName GetServerColNameByIndex(int columnIndex)
	{
		if (columnIndex < 0 || columnIndex >= lvServers.Columns.Count)
			return EServerColName.def;
		
		string columnText = lvServers.Columns[columnIndex].Text;
		return columnText switch
		{
			"No" => EServerColName.def,
			"类型" => EServerColName.configType,
			"别名" => EServerColName.remarks,
			"服务器地址" => EServerColName.address,
			"端口" => EServerColName.port,
			"加密方式" => EServerColName.security,
			"传输协议" => EServerColName.network,
			"TLS" => EServerColName.tls,
			"订阅" => EServerColName.subRemarks,
			"最低延迟" => EServerColName.minDelay,
			"平均延迟" => EServerColName.avgDelay,
			"平均速度" => EServerColName.testResult,
			"峰值速度" => EServerColName.MaxSpeed,
			"最后测速" => EServerColName.lastTestTime,
			_ => EServerColName.def
		};
	}

	private void menuExit_Click(object sender, EventArgs e)
	{
		base.Visible = false;
		Close();
		Application.Exit();
	}

	private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			ShowForm();
		}
	}

	private void ShowForm()
	{
		Show();
		base.WindowState = FormWindowState.Normal;
		Activate();
		base.ShowInTaskbar = true;
		txtMsgBox.ScrollToCaret();
		SetVisibleCore(value: true);
	}

	private void RefreshServers()
	{
		RefreshServersView();
	}

	public void RefreshServersView()
	{
		lvServers.BeginUpdate();
		lvServers.Items.Clear();
		int num = 0;
		
		for (int i = 0; i < config.vmess.Count; i++)
		{
			VmessItem vmessItem = config.vmess[i];
			if ((currentSubFilter == null || (currentSubFilter == "__unassigned__" && string.IsNullOrEmpty(vmessItem.subid)) || (currentSubFilter != "__unassigned__" && vmessItem.subid == currentSubFilter)) && MatchesKeywordFilter(vmessItem.remarks))
			{
				num++;
				
				// 按照列的物理顺序（Columns 集合中的顺序）添加数据，而不是显示顺序
				// 第一列
				string firstColKey = lvServers.Columns[0].Tag as string ?? "def";
				ListViewItem listViewItem = new ListViewItem(GetColumnValue(vmessItem, firstColKey, num));
				listViewItem.Name = firstColKey; // 设置第一列的 Name
				
				// 后续列按物理顺序添加
				for (int j = 1; j < lvServers.Columns.Count; j++)
				{
					string key = lvServers.Columns[j].Tag as string;
					if (!string.IsNullOrEmpty(key))
					{
						Utils.AddSubItem(listViewItem, key, GetColumnValue(vmessItem, key, num));
					}
				}
				
				listViewItem.Tag = i;
				if (num % 2 == 1)
				{
					listViewItem.BackColor = Color.WhiteSmoke;
				}
				if (listViewItem != null)
				{
					lvServers.Items.Add(listViewItem);
				}
			}
		}
		lvServers.EndUpdate();
		UpdateUnassignedTabVisibility();
	}

	private string GetColumnValue(VmessItem vmessItem, string columnKey, int rowNum)
	{
		return columnKey switch
		{
			"def" => rowNum.ToString(),
			"configType" => ((EConfigType)vmessItem.configType).ToString(),
			"remarks" => vmessItem.remarks,
			"address" => vmessItem.address,
			"port" => vmessItem.port.ToString(),
			"security" => vmessItem.security,
			"network" => vmessItem.network,
			"tls" => Utils.IsNullOrEmpty(vmessItem.streamSecurity) ? "none" : vmessItem.streamSecurity,
			"subRemarks" => vmessItem.getSubRemarks(config),
			"minDelay" => Utils.IsNullOrEmpty(vmessItem.minDelay) ? "" : vmessItem.minDelay,
			"avgDelay" => Utils.IsNullOrEmpty(vmessItem.avgDelay) ? "" : vmessItem.avgDelay,
			"testResult" => vmessItem.testResult,
			"MaxSpeed" => vmessItem.MaxSpeed,
			"lastTestTime" => vmessItem.lastTestTime,
			_ => ""
		};
	}

	private int GetColumnWidth(string key, int defaultValue)
	{
		if (config.uiItem.mainLvColWidth == null)
		{
			return defaultValue;
		}
		if (config.uiItem.mainLvColWidth.ContainsKey(key))
		{
			int num = config.uiItem.mainLvColWidth[key];
			if (num != 0)
			{
				return num;
			}
			return defaultValue;
		}
		return defaultValue;
	}

	private int GetColumnHeaderMinWidth(int columnIndex)
	{
		if (columnIndex < 0 || columnIndex >= lvServers.Columns.Count)
		{
			return 0;
		}
		return TextRenderer.MeasureText(lvServers.Columns[columnIndex].Text ?? string.Empty, lvServers.Font).Width + 20;
	}

	private int CalculateAutoColumnWidth(int columnIndex)
	{
		int num = GetColumnHeaderMinWidth(columnIndex);
		if (num == 0)
		{
			return 0;
		}
		
		// 获取列的 key
		string columnKey = lvServers.Columns[columnIndex].Tag as string;
		
		foreach (ListViewItem item in lvServers.Items)
		{
			string text;
			if (columnIndex == 0)
			{
				text = item.Text;
			}
			else if (!string.IsNullOrEmpty(columnKey) && item.SubItems.ContainsKey(columnKey))
			{
				text = item.SubItems[columnKey].Text;
			}
			else if (columnIndex < item.SubItems.Count)
			{
				text = item.SubItems[columnIndex].Text;
			}
			else
			{
				text = string.Empty;
			}
			
			int num2 = TextRenderer.MeasureText(text ?? string.Empty, lvServers.Font).Width + 20;
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	private void HideForm()
	{
		Hide();
		base.ShowInTaskbar = false;
		SetVisibleCore(value: false);
	}

	private bool isInitializing = false;

	private void MainForm_Load(object sender, EventArgs e)
	{
		try
		{
			string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
			if (File.Exists(text))
			{
				base.Icon = new Icon(text);
			}
		}
		catch
		{
		}
		ConfigHandler.LoadConfig(ref config);
		mihomoHandler = new MihomoHandler();
		mihomoHandler.ProcessEvent += mihomoHandler_ProcessEvent;
		mihomoHandler.ClashStart("-d ./config -ext-ctl 127.0.0.1:40001");
		subconverHandler = new SubconverHandler();
		subconverHandler.ProcessEvent += mihomoHandler_ProcessEvent;
		subconverHandler.SubconverStartNew();
		ConfigHandler.ProcessEvent += configHandler_ProcessEvent;
		TestParameter();
		InitServersView();
		InitSubscriptionTabs(restoreSelection: false);
		RestoreSelectedSubscriptionTab();
		RefreshServers();
		RestoreUI();
				mihomoHandler.LoadMihomoCore(config);
	}

	public void ShowMsg(string msg)
	{
		if (txtMsgBox.Lines.Length > 999)
		{
			ClearMsg();
		}
		txtMsgBox.AppendText(msg);
		if (!msg.EndsWith(Environment.NewLine))
		{
			txtMsgBox.AppendText(Environment.NewLine);
		}
	}

	private void ClearMsg()
	{
		txtMsgBox.Clear();
	}

	private void mihomoHandler_ProcessEvent(bool notify, string msg)
	{
		AppendText(notify, msg);
	}

	private void configHandler_ProcessEvent(bool notify, string msg)
	{
		AppendText(msg);
	}

	private void AppendText(bool notify, string msg)
	{
		try
		{
			AppendText(msg);
		}
		catch
		{
		}
	}

	private void AppendText(string text)
	{
		if (txtMsgBox.InvokeRequired)
		{
			Invoke(new AppendTextDelegate(AppendText), text);
		}
		else
		{
			ShowMsg(text);
		}
	}

	private void RestoreSelectedSubscriptionTab()
	{
		if (tabSubscriptions == null || tabSubscriptions.TabPages.Count == 0)
		{
			return;
		}
		string savedSubId = config.uiItem?.selectedSubId;
		if (string.IsNullOrEmpty(savedSubId) || savedSubId == "__all__")
		{
			tabSubscriptions.SelectedIndex = 0;
			currentSubFilter = null;
			return;
		}
		for (int i = 0; i < tabSubscriptions.TabPages.Count; i++)
		{
			if ((tabSubscriptions.TabPages[i].Tag as string) == savedSubId)
			{
				tabSubscriptions.SelectedIndex = i;
				currentSubFilter = savedSubId;
				return;
			}
		}
		tabSubscriptions.SelectedIndex = 0;
		currentSubFilter = null;
	}

	private void RestoreUI()
	{
		if (!config.uiItem.mainSize.IsEmpty && config.uiItem.mainSize.Width >= 400 && config.uiItem.mainSize.Height >= 300)
		{
			base.Size = config.uiItem.mainSize;
		}
		if (!config.uiItem.mainLocation.IsEmpty)
		{
			Point mainLocation = config.uiItem.mainLocation;
			if (mainLocation.X > -1000 && mainLocation.Y > -1000 && mainLocation.X < SystemInformation.VirtualScreen.Right && mainLocation.Y < SystemInformation.VirtualScreen.Bottom)
			{
				base.StartPosition = FormStartPosition.Manual;
				base.Location = mainLocation;
			}
		}
		if (config.uiItem.mainMaximized)
		{
			base.WindowState = FormWindowState.Maximized;
		}
		if (config.uiItem.splitterDistance > 100 && config.uiItem.splitterDistance < splitContainer1.Height - 100)
		{
			try
			{
				splitContainer1.SplitterDistance = config.uiItem.splitterDistance;
			}
			catch
			{
			}
		}
		for (int i = 0; i < lvServers.Columns.Count; i++)
		{
			EServerColName eServerColName = GetServerColNameByIndex(i);
			int val = ConfigHandler.GetformMainLvColWidth(ref config, eServerColName.ToString(), lvServers.Columns[i].Width);
			// 如果列被隐藏（宽度为0），保持隐藏状态
			if (IsColumnVisible(eServerColName.ToString()))
			{
				lvServers.Columns[i].Width = Math.Max(val, GetColumnHeaderMinWidth(i));
			}
		}
		if (!string.IsNullOrEmpty(config.uiItem.sortColumnKey))
		{
			currentSortColumnKey = config.uiItem.sortColumnKey;
			currentSortAscending = config.uiItem.sortAscending;
			SetSortIconByKey(currentSortColumnKey, currentSortAscending);
		}
	}

	private void EnsureColumnHeaderMinWidths()
	{
		for (int i = 0; i < lvServers.Columns.Count; i++)
		{
			// 跳过隐藏的列（宽度为0）
			if (lvServers.Columns[i].Width == 0)
				continue;
			int columnHeaderMinWidth = GetColumnHeaderMinWidth(i);
			if (columnHeaderMinWidth > 0 && lvServers.Columns[i].Width < columnHeaderMinWidth)
			{
				lvServers.Columns[i].Width = columnHeaderMinWidth;
			}
		}
	}

	private void TestParameter()
	{
		isInitializing = true;
		try
		{
			tbDelayUrl.Text = config.speedPingTestUrl ?? string.Empty;
			tbPingCount.Text = config.PingCount ?? "3";
			tbTimeout.Text = config.Timeout ?? "5";
			tbSpeedUrl.Text = config.speedTestUrl ?? string.Empty;
			tbDownloadThread.Text = config.DownloadThread ?? "4";
			tbDownloadMaxTime.Text = config.DownloadMaxTime ?? "10";
			tbKeywordFilter.Text = config.keywordFilter ?? string.Empty;
			cbKeywordFilter.Checked = config.keywordFilterEnabled;
			tbKeywordFilter.Enabled = config.keywordFilterEnabled;
		}
		finally
		{
			isInitializing = false;
		}
	}

	private bool DetectTunModeAndWarn()
	{
		if (Utils.TryDetectTunAdapter(out var adapterDisplayName))
		{
			string text = "检测到系统存在可能的代理 TUN/TAP 网卡：\n" + adapterDisplayName + "\n\n开启 TUN 模式会让测速结果包含代理链路导致结果失真，\n建议测速前关闭 TUN 模式或将本程序加入直连列表。\n\n是否仍要继续测速？";
			try
			{
				return MessageBox.Show(text, "检测到 TUN 模式", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes;
			}
			catch
			{
				return true;
			}
		}
		return true;
	}

	private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		switch (e.CloseReason)
		{
		case CloseReason.UserClosing:
			StorageUI();
			ConfigHandler.ToJsonFile(config);
			base.Visible = false;
			Application.Exit();
			break;
		case CloseReason.TaskManagerClosing:
		case CloseReason.FormOwnerClosing:
		case CloseReason.ApplicationExitCall:
			StorageUI();
			ConfigHandler.ToJsonFile(config);
			MyAppExit(blWindowsShutDown: false);
			break;
		case CloseReason.WindowsShutDown:
			StorageUI();
			ConfigHandler.ToJsonFile(config);
			MyAppExit(blWindowsShutDown: true);
			break;
		case CloseReason.MdiFormClosing:
			break;
		}
	}

	private void StorageUI()
	{
		if (base.WindowState == FormWindowState.Maximized)
		{
			config.uiItem.mainMaximized = true;
			if (!base.RestoreBounds.IsEmpty)
			{
				config.uiItem.mainSize = base.RestoreBounds.Size;
				config.uiItem.mainLocation = base.RestoreBounds.Location;
			}
		}
		else if (base.WindowState == FormWindowState.Normal)
		{
			config.uiItem.mainMaximized = false;
			config.uiItem.mainSize = new Size(base.Width, base.Height);
			config.uiItem.mainLocation = base.Location;
		}
		else if (!base.RestoreBounds.IsEmpty)
		{
			config.uiItem.mainSize = base.RestoreBounds.Size;
			config.uiItem.mainLocation = base.RestoreBounds.Location;
		}
		config.uiItem.splitterDistance = splitContainer1.SplitterDistance;
		config.uiItem.selectedSubId = (tabSubscriptions?.SelectedTab != null) ? ((tabSubscriptions.SelectedTab.Tag as string) ?? "__all__") : "__all__";
		config.uiItem.sortColumnKey = currentSortColumnKey;
		config.uiItem.sortAscending = currentSortAscending;
		for (int i = 0; i < lvServers.Columns.Count; i++)
		{
			EServerColName eServerColName = GetServerColNameByIndex(i);
			string key = eServerColName.ToString();
			// 跳过隐藏列，不保存宽度为0
			if (!IsColumnVisible(key))
				continue;
			ConfigHandler.AddformMainLvColWidth(ref config, key, lvServers.Columns[i].Width);
		}
	}

	private void MyAppExit(bool blWindowsShutDown)
	{
		try
		{
			mihomoHandler.MihomoStop();
		}
		catch
		{
		}
	}

	private void MainForm_Resize(object sender, EventArgs e)
	{
	}

	private void menuRealPingServer_Click(object sender, EventArgs e)
	{
		StartDelayTest(onlySelected: true);
	}

	private void menuDownLoadServer_Click(object sender, EventArgs e)
	{
		StartDownloadTest(onlySelected: true);
	}

	private bool PrepareTargetNodes(bool onlySelected)
	{
		if (onlySelected)
		{
			if (GetLvSelectedIndex() < 0 || lvSelecteds.Count == 0)
			{
				return false;
			}
			return true;
		}

		// 主页按钮点击：若多选了节点则测试选中节点，否则测试当前列表全部节点
		lvSelecteds.Clear();
		if (lvServers.SelectedItems.Count > 1)
		{
			foreach (ListViewItem selectedItem in lvServers.SelectedItems)
			{
				if (selectedItem.Tag != null)
					lvSelecteds.Add((int)selectedItem.Tag);
			}
		}
		else
		{
			foreach (ListViewItem item in lvServers.Items)
			{
				if (item.Tag != null)
					lvSelecteds.Add((int)item.Tag);
			}
		}

		if (lvSelecteds.Count == 0)
		{
			UI.Show("当前列表没有可测试的节点", this);
			return false;
		}
		return true;
	}

	private void StartDelayTest(bool onlySelected)
	{
		if (isTestingDelay)
		{
			cts?.Cancel();
			return;
		}
		if (isTestingSpeed)
		{
			UI.Show("当前正在进行下载测试，请先停止后再测试延迟", this);
			return;
		}
		if (!PrepareTargetNodes(onlySelected))
		{
			return;
		}
		if (!DetectTunModeAndWarn())
		{
			AppendText(notify: false, "已取消延迟测试（检测到TUN模式）");
			return;
		}
		try
		{
			createYamlConfig();
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "出现异常");
			return;
		}

		isTestingDelay = true;
		btnTestDelay.Text = "停止测试";
		btnTestSpeed.Enabled = false;
		cts = new CancellationTokenSource();

		foreach (int idx in lvSelecteds)
		{
			SetDelayResult(idx, "测试中...", "测试中...");
		}

		AppendText(notify: false, $"开始延迟测试（共 {lvSelecteds.Count} 个节点，无线程限制全并发，每节点测试 {config.PingCount} 次）...");
		CancellationToken token = cts.Token;
		SpeedtestHandler handler = new SpeedtestHandler(config, mihomoHandler, new List<int>(lvSelecteds), UpdateDelayHandler, null, delegate
		{
			if (base.IsHandleCreated)
			{
				BeginInvoke((Action)delegate
				{
					isTestingDelay = false;
					btnTestDelay.Text = "测试延迟";
					btnTestSpeed.Enabled = true;
					SortListView();
					ConfigHandler.ToJsonFile(config);
				});
			}
		});
		Task.Run(delegate
		{
			handler.RunDelayTest(token);
		});
	}

	private void StartDownloadTest(bool onlySelected)
	{
		if (isTestingSpeed)
		{
			cts?.Cancel();
			return;
		}
		if (isTestingDelay)
		{
			UI.Show("当前正在进行延迟测试，请先停止后再测试下载", this);
			return;
		}
		if (!PrepareTargetNodes(onlySelected))
		{
			return;
		}
		if (!DetectTunModeAndWarn())
		{
			AppendText(notify: false, "已取消下载测速（检测到TUN模式）");
			return;
		}
		try
		{
			createYamlConfig();
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "出现异常");
			return;
		}

		isTestingSpeed = true;
		btnTestSpeed.Text = "停止测试";
		btnTestDelay.Enabled = false;
		cts = new CancellationTokenSource();

		foreach (int idx in lvSelecteds)
		{
			SetSpeedResult(idx, "等待测试...", "");
		}

		AppendText(notify: false, $"开始下载测速（共 {lvSelecteds.Count} 个节点，并发线程：{config.DownloadThread}，最大时间：{config.DownloadMaxTime} 秒）...");
		CancellationToken token = cts.Token;
		SpeedtestHandler handler = new SpeedtestHandler(config, mihomoHandler, new List<int>(lvSelecteds), null, UpdateSpeedHandler, delegate
		{
			if (base.IsHandleCreated)
			{
				BeginInvoke((Action)delegate
				{
					isTestingSpeed = false;
					btnTestSpeed.Text = "测试下载";
					btnTestDelay.Enabled = true;
					SortListView();
					ConfigHandler.ToJsonFile(config);
				});
			}
		});
		Task.Run(delegate
		{
			handler.RunDownloadTest(token);
		});
	}

	private void SetDelayResult(int k, string minTxt, string avgTxt)
	{
		if (k < 0 || k >= config.vmess.Count)
		{
			return;
		}
		config.vmess[k].minDelay = minTxt;
		config.vmess[k].avgDelay = avgTxt;
		foreach (ListViewItem item in lvServers.Items)
		{
			if (item.Tag != null && (int)item.Tag == k)
			{
				if (item.SubItems.ContainsKey("minDelay"))
					item.SubItems["minDelay"].Text = minTxt;
				if (item.SubItems.ContainsKey("avgDelay"))
					item.SubItems["avgDelay"].Text = avgTxt;
				break;
			}
		}
		if (((minTxt != null && minTxt.Contains("ms")) || (avgTxt != null && avgTxt.Contains("ms"))) && (minTxt == null || !minTxt.Contains("测试中")))
		{
			SetLastTestTime(k, DateTime.Now.ToString("MM-dd HH:mm:ss"));
		}
	}

	private void SetSpeedResult(int k, string avgTxt, string maxTxt)
	{
		if (k < 0 || k >= config.vmess.Count)
		{
			return;
		}
		config.vmess[k].testResult = avgTxt;
		config.vmess[k].MaxSpeed = maxTxt;
		foreach (ListViewItem item in lvServers.Items)
		{
			if (item.Tag != null && (int)item.Tag == k)
			{
				if (item.SubItems.ContainsKey("testResult"))
					item.SubItems["testResult"].Text = avgTxt;
				if (item.SubItems.ContainsKey("MaxSpeed"))
					item.SubItems["MaxSpeed"].Text = maxTxt;
				break;
			}
		}
		if (avgTxt != null && avgTxt.Contains("Mbps") && !avgTxt.Contains("测速中"))
		{
			SetLastTestTime(k, DateTime.Now.ToString("MM-dd HH:mm:ss"));
		}
	}

	private void SetLastTestTime(int k, string txt)
	{
		if (k < 0 || k >= config.vmess.Count)
		{
			return;
		}
		config.vmess[k].lastTestTime = txt;
		foreach (ListViewItem item in lvServers.Items)
		{
			if (item.Tag != null && (int)item.Tag == k)
			{
				if (item.SubItems.ContainsKey("lastTestTime"))
				{
					item.SubItems["lastTestTime"].Text = txt;
				}
				break;
			}
		}
	}

	public void createYamlConfig()
	{
		int externalControllerPort = config.externalControllerPort;
		string text = Guid.NewGuid().ToString("N");
		string path = Utils.GetPath("subconverter\\temp_" + text + ".txt");
		string text2 = "";
		string text3 = "";
		string path2 = Utils.GetPath("config\\temp.yaml");
		try
		{
			if (!ConfigHandler.TcpClientCheck("127.0.0.1", externalControllerPort))
			{
				throw new Exception("无法链接到Mihomo内核，请确认：\n1. mihomo-nodes.exe 文件是否存在于程序目录\n2. 端口 " + externalControllerPort + " 是否被占用\n3. 尝试重新打开测速软件");
			}
			if (!ConfigHandler.TcpClientCheck("127.0.0.1", 25500))
			{
				throw new Exception("无法链接到subconverter(端口:25500)，请确认：\n1. subconverter\\subconverter.exe 文件是否存在\n2. 端口 25500 是否被占用\n3. 尝试重新打开测速软件");
			}
			StringBuilder stringBuilder = new StringBuilder();
			List<VmessItem> listAnyTLS = new List<VmessItem>();
			List<string> listAnyTLSNames = new List<string>();
			List<VmessItem> listVlessReality = new List<VmessItem>();
			List<string> listVlessRealityNames = new List<string>();
			foreach (int lvSelected in lvSelecteds)
			{
				if (lvSelected >= 0 && lvSelected < config.vmess.Count)
				{
					VmessItem vmessItem = config.vmess[lvSelected];
					// AnyTLS 节点直接生成 YAML
					if (vmessItem.configType == 12)
					{
						string item = (config.GetLocalPort() + lvSelected).ToString();
						listAnyTLS.Add(vmessItem);
						listAnyTLSNames.Add(item);
						continue;
					}
					// VLESS 节点全部直接生成 YAML（绕过不支持 VLESS 的 subconverter）
					if (vmessItem.configType == 5)
					{
						// 跳过有问题的 Reality 节点（缺 publicKey 或缺 shortId）
						bool isBrokenReality = vmessItem.streamSecurity == "reality" && 
							(string.IsNullOrEmpty(vmessItem.publicKey) || string.IsNullOrEmpty(vmessItem.shortId));
						if (isBrokenReality)
						{
							continue;
						}
						string item = (config.GetLocalPort() + lvSelected).ToString();
						listVlessReality.Add(vmessItem);
						listVlessRealityNames.Add(item);
						continue;
					}
				}
				string shareUrl = ShareHandler.GetShareUrl(config, lvSelected, changRemark: true);
				if (!Utils.IsNullOrEmpty(shareUrl))
				{
					stringBuilder.Append(shareUrl);
					stringBuilder.AppendLine();
				}
			}
			string text4 = "";
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Insert(0, "ss://YWVzLTI1Ni1nY206ZmFCQW9ENTRrODdVSkc3QDEuMS4xLjE6NjY2#%e5%8d%a0%e4%bd%8d%e8%8a%82%e7%82%b9" + Environment.NewLine);
				File.WriteAllText(path, Utils.Base64Encode(stringBuilder.ToString()), Encoding.UTF8);
				HttpWebRequest obj = (HttpWebRequest)WebRequest.Create($"http://127.0.0.1:{25500}/sub?target=clash&url=temp_{text}.txt&insert=false&list=true");
				obj.Method = "GET";
				obj.Timeout = 10000;
				obj.ReadWriteTimeout = 10000;
				obj.ContinueTimeout = 10000;
				using StreamReader streamReader = new StreamReader(obj.GetResponse().GetResponseStream(), Encoding.UTF8);
				text4 = streamReader.ReadToEnd();
				if (text4.Contains("proxies: ~"))
				{
					text4 = text4.Replace("proxies: ~", "proxies:");
				}
			}
			else
			{
				text4 = "mixed-port: 40000\r\nmode: Global\r\nlog-level: info\r\nexternal-controller: 127.0.0.1:" + externalControllerPort + "\r\nproxies:\r\nproxy-groups:\r\n  - name: PROXY\r\n    type: select\r\n    proxies:\r\nrules:\r\n  - MATCH,PROXY\r\n";
			}
			// 生成 AnyTLS 节点的 YAML 配置
			if (listAnyTLS.Count > 0)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				StringBuilder stringBuilder3 = new StringBuilder();
				for (int i = 0; i < listAnyTLS.Count; i++)
				{
					VmessItem vmessItem2 = listAnyTLS[i];
					string text5 = listAnyTLSNames[i];
					bool flag = vmessItem2.allowInsecure == "true" || vmessItem2.allowInsecure == "1";
					stringBuilder2.AppendLine("  - name: \"" + text5 + "\"");
					stringBuilder2.AppendLine("    type: anytls");
					stringBuilder2.AppendLine("    server: " + vmessItem2.address);
					stringBuilder2.AppendLine($"    port: {vmessItem2.port}");
					stringBuilder2.AppendLine("    password: \"" + vmessItem2.id + "\"");
					if (!string.IsNullOrEmpty(vmessItem2.sni))
					{
						stringBuilder2.AppendLine("    sni: " + vmessItem2.sni);
					}
					stringBuilder2.AppendLine("    skip-cert-verify: " + flag.ToString().ToLower());
					stringBuilder2.AppendLine("    udp: true");
					stringBuilder3.AppendLine("      - \"" + text5 + "\"");
				}
				int num = text4.IndexOf("proxies:");
				if (num >= 0)
				{
					int startIndex = text4.IndexOf("\n", num) + 1;
					text4 = text4.Insert(startIndex, stringBuilder2.ToString());
				}
				int num2 = text4.IndexOf("proxy-groups:");
				if (num2 >= 0)
				{
					int num3 = text4.IndexOf("proxies:", num2);
					if (num3 >= 0)
					{
						int startIndex2 = text4.IndexOf("\n", num3) + 1;
						text4 = text4.Insert(startIndex2, stringBuilder3.ToString());
					}
				}
			}
			// 生成 VLESS 节点的 YAML 配置
			if (listVlessReality.Count > 0)
			{
				StringBuilder sbVlessProxies = new StringBuilder();
				StringBuilder sbVlessProxyNames = new StringBuilder();
				for (int i = 0; i < listVlessReality.Count; i++)
				{
					AppendVlessNodeYaml(sbVlessProxies, sbVlessProxyNames, listVlessReality[i], listVlessRealityNames[i]);
				}
				
				int num = text4.IndexOf("proxies:");
				if (num >= 0)
				{
					int startIndex = text4.IndexOf("\n", num) + 1;
					text4 = text4.Insert(startIndex, sbVlessProxies.ToString());
				}
				int num2 = text4.IndexOf("proxy-groups:");
				if (num2 >= 0)
				{
					int num3 = text4.IndexOf("proxies:", num2);
					if (num3 >= 0)
					{
						int startIndex2 = text4.IndexOf("\n", num3) + 1;
						text4 = text4.Insert(startIndex2, sbVlessProxyNames.ToString());
					}
				}
			}
			int totalNativeNodes = listAnyTLS.Count + listVlessReality.Count;
			if (string.IsNullOrEmpty(text4) || (!text4.Contains("proxies:") && totalNativeNodes == 0))
			{
				throw new Exception("没有可用测速节点");
			}
			text4 = EnsureUnifiedDelayInYaml(text4);
			text4 = InjectPerNodeListenersInYaml(text4);
			File.WriteAllText(path2, text4, Encoding.UTF8);
			string nativeNodeInfo = "";
			if (listAnyTLS.Count > 0) nativeNodeInfo += $"{listAnyTLS.Count} 个 AnyTLS";
			if (listVlessReality.Count > 0)
			{
				if (nativeNodeInfo.Length > 0) nativeNodeInfo += ", ";
				nativeNodeInfo += $"{listVlessReality.Count} 个 VLESS";
			}
			if (nativeNodeInfo.Length > 0)
			{
				ShowMsg($"Mihomo配置文件生成成功（含 {nativeNodeInfo} 节点）");
			}
			else
			{
				ShowMsg("Mihomo配置文件生成成功");
			}
			string args = JsonConvert.SerializeObject(new
			{
				path = Path.GetFullPath(path2).Replace("\\", "/")
			}, Formatting.None);
			text3 = "http://127.0.0.1:" + externalControllerPort + "/configs";
			text2 = ConfigHandler.sendReq(args, text3, "PUT");
			if (text2 != "204")
			{
				throw new Exception("切换测速配置文件失败！" + text2);
			}
			// 检查 Mihomo 内核是否仍存活
			if (!ConfigHandler.TcpClientCheck("127.0.0.1", int.Parse(config.externalController.Split(':')[1])))
			{
				throw new Exception("Mihomo内核加载配置后异常退出，可能存在不支持的节点格式");
			}
			ShowMsg("切换测速配置文件成功！");
			text2 = ConfigHandler.sendReq(JsonConvert.SerializeObject(new
			{
				port = 40000
			}, Formatting.None), text3, "PATCH");
			if (text2 != "204")
			{
				throw new Exception("切换http代理端口失败！" + text2);
			}
			ShowMsg("切换http代理端口成功！");
			text2 = ConfigHandler.sendReq(JsonConvert.SerializeObject(new Dictionary<string, object>
			{
				{ "mode", "Global" },
				{ "unified-delay", true }
			}, Formatting.None), text3, "PATCH");
			if (text2 != "204")
			{
				throw new Exception("切换全局代理失败！" + text2);
			}
			ShowMsg("切换全局代理成功！");
		}
		finally
		{
			try
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch
			{
			}
		}
	}

	private static string EnsureUnifiedDelayInYaml(string yamlContent)
	{
		if (string.IsNullOrEmpty(yamlContent))
		{
			return yamlContent;
		}
		if (Regex.IsMatch(yamlContent, @"(?m)^\s*unified-delay\s*:"))
		{
			return Regex.Replace(yamlContent, @"(?m)^(\s*unified-delay\s*:\s*).*$", "${1}true");
		}
		return "unified-delay: true\r\n" + yamlContent;
	}

	private static string InjectPerNodeListenersInYaml(string yamlContent)
	{
		if (string.IsNullOrEmpty(yamlContent))
		{
			return yamlContent;
		}
		int proxiesIdx = yamlContent.IndexOf("proxies:");
		if (proxiesIdx < 0)
		{
			return yamlContent;
		}
		int groupsIdx = yamlContent.IndexOf("proxy-groups:", proxiesIdx);
		string proxiesSection = (groupsIdx > proxiesIdx)
			? yamlContent.Substring(proxiesIdx, groupsIdx - proxiesIdx)
			: yamlContent.Substring(proxiesIdx);

		MatchCollection matches = Regex.Matches(proxiesSection, @"(?m)^\s*-\s*(?:\{[^\r\n]*?\b)?name\s*:\s*[""']?(\d{5,6})[""']?(?:\s*[,}]|\s*$)");
		HashSet<int> ports = new HashSet<int>();
		StringBuilder sbListeners = new StringBuilder();
		sbListeners.AppendLine("listeners:");
		foreach (Match match in matches)
		{
			if (int.TryParse(match.Groups[1].Value, out int port) && port >= 10000 && port <= 65535 && ports.Add(port))
			{
				sbListeners.AppendLine($"  - name: in_{port}");
				sbListeners.AppendLine("    type: mixed");
				sbListeners.AppendLine($"    port: {port}");
				sbListeners.AppendLine("    listen: 127.0.0.1");
				sbListeners.AppendLine($"    proxy: \"{port}\"");
			}
		}
		if (ports.Count == 0)
		{
			return yamlContent;
		}
		return yamlContent.Insert(proxiesIdx, sbListeners.ToString());
	}

	private void UpdateDelayHandler(VmessItem node, string minDelay, string avgDelay, bool isNodeFinished)
	{
		if (lvServers.IsHandleCreated && node != null)
		{
			lvServers.BeginInvoke((MethodInvoker)delegate
			{
				int k = config.vmess.IndexOf(node);
				if (k >= 0)
				{
					SetDelayResult(k, minDelay, avgDelay);
					if (isNodeFinished)
					{
						SortListView();
					}
				}
			});
		}
	}

	private void UpdateSpeedHandler(VmessItem node, string avgSpeed, string maxSpeed, bool isNodeFinished)
	{
		if (lvServers.IsHandleCreated && node != null)
		{
			lvServers.BeginInvoke((MethodInvoker)delegate
			{
				int k = config.vmess.IndexOf(node);
				if (k >= 0)
				{
					SetSpeedResult(k, avgSpeed, maxSpeed);
					if (isNodeFinished)
					{
						SortListView();
					}
				}
			});
		}
	}

	private int GetLvSelectedIndex()
	{
		int result = -1;
		lvSelecteds.Clear();
		try
		{
			if (lvServers.SelectedIndices.Count <= 0)
			{
				UI.Show("请先选择需要测速的服务器", this);
				return result;
			}
			result = ((lvServers.SelectedItems.Count <= 0 || lvServers.SelectedItems[0].Tag == null) ? lvServers.SelectedIndices[0] : ((int)lvServers.SelectedItems[0].Tag));
			foreach (ListViewItem selectedItem in lvServers.SelectedItems)
			{
				if (selectedItem.Tag != null)
				{
					lvSelecteds.Add((int)selectedItem.Tag);
				}
				else
				{
					lvSelecteds.Add(lvServers.Items.IndexOf(selectedItem));
				}
			}
			return result;
		}
		catch
		{
			return result;
		}
	}

	private bool MatchesKeywordFilter(string remarks)
	{
		if (config == null || !config.keywordFilterEnabled)
		{
			return true;
		}
		string text = tbKeywordFilter?.Text?.Trim();
		if (string.IsNullOrEmpty(text))
		{
			return true;
		}
		if (string.IsNullOrEmpty(remarks))
		{
			return false;
		}
		string[] array = text.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		bool flag = false;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text2 = array2[i].Trim();
			if (text2.StartsWith("!"))
			{
				string value = text2.Substring(1);
				if (!string.IsNullOrEmpty(value) && remarks.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return false;
				}
			}
			else
			{
				flag = true;
				if (remarks.IndexOf(text2, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}
		}
		return !flag;
	}

	private async void menuAddServers_Click(object sender, EventArgs e)
	{
		string clipboardData = Utils.GetClipboardData();
		if (string.IsNullOrWhiteSpace(clipboardData))
		{
			return;
		}
		string text = clipboardData.Trim();
		// 检测是否包含订阅链接和节点链接
		List<string> subscriptionUrls = ExtractSubscriptionUrls(text);
		string nodeLinks = ExtractNodeLinks(text);
		// 如果只有订阅链接（无节点链接）
		if (subscriptionUrls.Count > 0 && string.IsNullOrWhiteSpace(nodeLinks))
		{
			if (subscriptionUrls.Count == 1)
			{
				ImportFromSubscriptionUrl(subscriptionUrls[0]);
			}
			else
			{
				// 多个订阅链接，弹出确认对话框
				DialogResult result = MessageBox.Show(
					$"检测到剪贴板中有 {subscriptionUrls.Count} 个订阅链接，是否全部导入？\n\n" +
					string.Join("\n", subscriptionUrls.Select((url, i) => $"{i + 1}. {(url.Length > 60 ? url.Substring(0, 60) + "..." : url)}")),
					"批量导入订阅",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					ImportFromMultipleSubscriptionUrls(subscriptionUrls);
				}
			}
			return;
		}
		// 如果同时包含订阅链接和节点链接（混合内容）
		if (subscriptionUrls.Count > 0 && !string.IsNullOrWhiteSpace(nodeLinks))
		{
			DialogResult result = MessageBox.Show(
				$"检测到剪贴板中包含混合内容：\n\n" +
				$"• 订阅链接: {subscriptionUrls.Count} 个\n" +
				$"• 节点链接: 已检测到\n\n" +
				$"是否全部导入？（先导入订阅，再导入节点）",
				"混合内容导入",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
			if (result == DialogResult.Yes)
			{
				await ImportMixedContentAsync(subscriptionUrls, nodeLinks);
			}
			return;
		}
		// 只有节点链接（无订阅），走原有逻辑
		AppendText(notify: false, "正在处理节点...");
		int num = await Task.Run(() => MainFormHandler.Instance.AddBatchServers(config, clipboardData));
		if (num > 0)
		{
			RefreshServers();
			ConfigHandler.ToJsonFile(config);
			UI.Show($"成功从剪贴板导入{num}个节点", this);
		}
		else
		{
			AppendText(notify: false, "没有导入任何节点");
		}
	}

	/// <summary>
	/// 从文本中提取订阅链接（排除节点分享链接）
	/// </summary>
	private List<string> ExtractSubscriptionUrls(string text)
	{
		List<string> urls = new List<string>();
		// 节点协议前缀
		string[] nodeProtocols = { "vmess://", "vless://", "ss://", "ssr://", "trojan://", "hysteria2://", "hy2://", "anytls://", "socks://" };
		// 按行分割
		string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string line in lines)
		{
			string trimmedLine = line.Trim();
			// 检查是否是 http/https 链接
			if ((trimmedLine.StartsWith("http://") || trimmedLine.StartsWith("https://")))
			{
				// 排除包含节点协议的链接（可能是混合内容）
				bool containsNodeProtocol = nodeProtocols.Any(p => trimmedLine.Contains(p));
				if (!containsNodeProtocol)
				{
					urls.Add(trimmedLine);
				}
			}
		}
		return urls;
	}

	/// <summary>
	/// 从文本中提取节点链接（排除订阅链接）
	/// </summary>
	private string ExtractNodeLinks(string text)
	{
		string[] nodeProtocols = { "vmess://", "vless://", "ss://", "ssr://", "trojan://", "hysteria2://", "hy2://", "anytls://", "socks://" };
		List<string> nodeLines = new List<string>();
		string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string line in lines)
		{
			string trimmedLine = line.Trim();
			// 检查是否以节点协议开头
			if (nodeProtocols.Any(p => trimmedLine.StartsWith(p)))
			{
				nodeLines.Add(trimmedLine);
			}
		}
		return string.Join("\n", nodeLines);
	}

	/// <summary>
	/// 导入混合内容（订阅+节点）
	/// </summary>
	private async Task ImportMixedContentAsync(List<string> subscriptionUrls, string nodeLinks)
	{
		int totalNodes = 0;
		int subSuccessCount = 0;
		int subFailCount = 0;
		AppendText(notify: false, $"开始导入混合内容...");
		// 先导入订阅
		AppendText(notify: false, $"\n--- 导入订阅链接 ({subscriptionUrls.Count} 个) ---");
		for (int i = 0; i < subscriptionUrls.Count; i++)
		{
			string url = subscriptionUrls[i];
			AppendText(notify: false, $"[{i + 1}/{subscriptionUrls.Count}] 正在处理: {(url.Length > 60 ? url.Substring(0, 60) + "..." : url)}");
			try
			{
				int nodesBefore = config.vmess?.Count ?? 0;
				bool success = await ImportFromSubscriptionUrlAsync(url);
				if (success)
				{
					int nodesAdded = (config.vmess?.Count ?? 0) - nodesBefore;
					totalNodes += nodesAdded;
					subSuccessCount++;
					AppendText(notify: false, $"--> 成功导入 {nodesAdded} 个节点");
				}
				else
				{
					subFailCount++;
					AppendText(notify: false, "--> 导入失败");
				}
			}
			catch (Exception ex)
			{
				subFailCount++;
				AppendText(notify: false, $"--> 导入异常: {ex.Message}");
			}
		}
		// 再导入节点链接
		AppendText(notify: false, $"\n--- 导入节点链接 ---");
		int nodeCountBefore = config.vmess?.Count ?? 0;
		int directNodes = await Task.Run(() => MainFormHandler.Instance.AddBatchServers(config, nodeLinks));
		if (directNodes > 0)
		{
			totalNodes += directNodes;
			AppendText(notify: false, $"--> 成功导入 {directNodes} 个节点");
		}
		else
		{
			AppendText(notify: false, "--> 没有导入任何节点");
		}
		// 汇总
		AppendText(notify: false, $"\n混合内容导入完成！\n订阅: 成功 {subSuccessCount}, 失败 {subFailCount}\n节点: {directNodes} 个\n共导入 {totalNodes} 个节点");
		if (totalNodes > 0)
		{
			InitSubscriptionTabs();
			RefreshServersView();
			ShowMsg($"混合内容导入完成\n订阅: 成功 {subSuccessCount}, 失败 {subFailCount}\n节点链接: {directNodes} 个\n共导入 {totalNodes} 个节点");
		}
		else
		{
			ShowMsg("混合内容导入完成，但没有成功导入任何节点");
		}
	}

	/// <summary>
	/// 批量导入多个订阅链接
	/// </summary>
	private async void ImportFromMultipleSubscriptionUrls(List<string> urls)
	{
		int totalNodes = 0;
		int successCount = 0;
		int failCount = 0;
		int nodeCountBefore = config.vmess?.Count ?? 0;
		AppendText(notify: false, $"开始批量导入 {urls.Count} 个订阅...");
		for (int i = 0; i < urls.Count; i++)
		{
			string url = urls[i];
			AppendText(notify: false, $"\n[{i + 1}/{urls.Count}] 正在处理: {(url.Length > 80 ? url.Substring(0, 80) + "..." : url)}");
			try
			{
				int nodesBefore = config.vmess?.Count ?? 0;
				bool success = await ImportFromSubscriptionUrlAsync(url);
				if (success)
				{
					int nodesAdded = (config.vmess?.Count ?? 0) - nodesBefore;
					totalNodes += nodesAdded;
					successCount++;
					AppendText(notify: false, $"--> 成功导入 {nodesAdded} 个节点");
				}
				else
				{
					failCount++;
					AppendText(notify: false, "--> 导入失败");
				}
			}
			catch (Exception ex)
			{
				failCount++;
				AppendText(notify: false, $"--> 导入异常: {ex.Message}");
			}
		}
		AppendText(notify: false, $"\n批量导入完成！成功: {successCount}, 失败: {failCount}, 共导入 {totalNodes} 个节点");
		if (totalNodes > 0)
		{
			InitSubscriptionTabs();
			RefreshServersView();
			ShowMsg($"批量导入完成\n成功: {successCount} 个订阅\n失败: {failCount} 个订阅\n共导入 {totalNodes} 个节点");
		}
		else
		{
			ShowMsg($"批量导入完成，但没有成功导入任何节点\n失败: {failCount} 个订阅");
		}
	}

	/// <summary>
	/// 异步导入单个订阅链接
	/// </summary>
	private Task<bool> ImportFromSubscriptionUrlAsync(string url)
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		DownloadHandle downloadHandle = new DownloadHandle();
		downloadHandle.Error += delegate(object obj, ErrorEventArgs args)
		{
			AppendText(notify: false, "--> 下载失败：" + args.GetException().Message);
			tcs.TrySetResult(false);
		};
		downloadHandle.UpdateCompleted += delegate(object obj, DownloadHandle.ResultEventArgs args)
		{
			if (args.Success)
			{
				string decoded = Utils.Base64Decode(args.Msg);
				string content = !Utils.IsNullOrEmpty(decoded) ? decoded : args.Msg;
				if (MainFormHandler.Instance.AddBatchServers(config, content) > 0)
				{
					tcs.TrySetResult(true);
				}
				else
				{
					// 如果解码后导入失败，尝试原始内容
					if (!Utils.IsNullOrEmpty(decoded) && MainFormHandler.Instance.AddBatchServers(config, args.Msg) > 0)
					{
						tcs.TrySetResult(true);
					}
					else
					{
						tcs.TrySetResult(false);
					}
				}
			}
			else
			{
				tcs.TrySetResult(false);
			}
		};
		downloadHandle.WebDownloadString(url);
		return tcs.Task;
	}

	private void ImportFromSubscriptionUrl(string url)
	{
		int nodeCountBefore = config.vmess?.Count ?? 0;
		AppendText(notify: false, "检测到订阅链接，开始导入...");
		AppendText(notify: false, "--> URL: " + url);
		AppendText(notify: false, "--> 正在下载订阅数据...");
		DownloadHandle downloadHandle = new DownloadHandle();
		downloadHandle.Error += delegate(object obj, ErrorEventArgs args)
		{
			AppendText(notify: false, "--> 下载失败：" + args.GetException().Message);
			ShowMsg("下载订阅失败：" + args.GetException().Message);
		};
		downloadHandle.UpdateCompleted += delegate(object obj, DownloadHandle.ResultEventArgs args)
		{
			if (args.Success)
			{
				AppendText(notify: false, "--> 获取订阅数据成功，数据长度：" + args.Msg.Length);
				string text = Utils.Base64Decode(args.Msg);
				if (!Utils.IsNullOrEmpty(text))
				{
					AppendText(notify: false, "--> Base64解码成功");
					if (text.IndexOf("vmess://") != -1 || text.IndexOf("vless://") != -1 || text.IndexOf("ss://") != -1 || text.IndexOf("ssr://") != -1 || text.IndexOf("trojan://") != -1 || text.IndexOf("socks://") != -1 || text.IndexOf("hysteria2://") != -1 || text.IndexOf("hy2://") != -1 || text.IndexOf("anytls://") != -1)
					{
						if (MainFormHandler.Instance.AddBatchServers(config, text) <= 0)
						{
							AppendText(notify: false, "--> 导入节点信息失败");
							ShowMsg("导入节点信息失败");
						}
						else
						{
							int num = (config.vmess?.Count ?? 0) - nodeCountBefore;
							AppendText(notify: false, $"--> 成功导入 {num} 个节点");
							InitSubscriptionTabs();
							RefreshServersView();
							ShowMsg($"从订阅链接导入了 {num} 个节点");
						}
					}
					else
					{
						AppendText(notify: false, "--> 尝试作为非base64格式导入...");
						if (MainFormHandler.Instance.AddBatchServers(config, args.Msg) > 0)
						{
							int num2 = (config.vmess?.Count ?? 0) - nodeCountBefore;
							AppendText(notify: false, $"--> 成功导入 {num2} 个节点");
							InitSubscriptionTabs();
							RefreshServersView();
							ShowMsg($"从订阅链接导入了 {num2} 个节点");
						}
						else
						{
							AppendText(notify: false, "--> 订阅内容格式不正确，无法识别的节点格式");
							ShowMsg("订阅内容格式不正确");
						}
					}
				}
				else
				{
					AppendText(notify: false, "--> Base64解码失败，尝试直接导入原始内容...");
					if (MainFormHandler.Instance.AddBatchServers(config, args.Msg) > 0)
					{
						int num3 = (config.vmess?.Count ?? 0) - nodeCountBefore;
						AppendText(notify: false, $"--> 成功导入 {num3} 个节点");
						InitSubscriptionTabs();
						RefreshServersView();
						ShowMsg($"从订阅链接导入了 {num3} 个节点");
					}
					else
					{
						AppendText(notify: false, "--> 订阅内容为空或格式不正确");
						ShowMsg("订阅内容为空或格式不正确");
					}
				}
			}
			else
			{
				AppendText(notify: false, "--> 导入失败！" + args.Msg);
				ShowMsg("导入失败：" + args.Msg);
			}
		};
		downloadHandle.WebDownloadString(url);
	}

	private void menuSelectAll_Click(object sender, EventArgs e)
	{
		foreach (ListViewItem item in lvServers.Items)
		{
			item.Selected = true;
		}
	}

	private void menuEditServer_Click(object sender, EventArgs e)
	{
		EditSelectedServer();
	}

	private void lvServers_DoubleClick(object sender, EventArgs e)
	{
		EditSelectedServer();
	}

	private void EditSelectedServer()
	{
		if (GetLvSelectedIndex() < 0)
		{
			UI.Show("请先选择需要编辑的节点", this);
			return;
		}
		if (lvSelecteds.Count > 1)
		{
			UI.Show("编辑功能仅支持单选，请只选择一个节点", this);
			return;
		}
		int num = lvSelecteds[0];
		if (num >= 0 && num < config.vmess.Count)
		{
			EditServerForm editServerForm = new EditServerForm(config.vmess[num], num);
			if (editServerForm.ShowDialog(this) == DialogResult.OK)
			{
				config.vmess[num] = editServerForm.ResultItem;
				RefreshServers();
				ConfigHandler.ToJsonFile(config);
				AppendText(notify: false, "节点 [" + editServerForm.ResultItem.remarks + "] 已更新");
			}
		}
	}

	private void menuRemoveServer_Click(object sender, EventArgs e)
	{
		if (GetLvSelectedIndex() >= 0 && UI.ShowYesNo("是否删除选中的节点？", this) != DialogResult.No)
		{
			for (int num = lvSelecteds.Count - 1; num >= 0; num--)
			{
				ConfigHandler.RemoveServer(ref config, lvSelecteds[num]);
			}
			RefreshServers();
			ConfigHandler.ToJsonFile(config);
		}
	}

	private void menuRemoveDuplicateServer_Click(object sender, EventArgs e)
	{
		Utils.DedupServerList(config.vmess, out var result, keepOlder: true);
		int count = config.vmess.Count;
		int count2 = result.Count;
		if (result != null)
		{
			config.vmess = result;
		}
		RefreshServers();
		ConfigHandler.ToJsonFile(config);
		UI.Show($"执行完成。已删除{count - count2}个重复节点", this);
	}

	private void menuExport2ShareUrl_Click(object sender, EventArgs e)
	{
		GetLvSelectedIndex();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int lvSelected in lvSelecteds)
		{
			string shareUrl = ShareHandler.GetShareUrl(config, lvSelected);
			if (!Utils.IsNullOrEmpty(shareUrl))
			{
				stringBuilder.Append(shareUrl);
				stringBuilder.AppendLine();
			}
		}
		if (stringBuilder.Length > 0)
		{
			Utils.SetClipboardData(stringBuilder.ToString());
			AppendText(notify: false, "节点URL已复制到剪贴板");
		}
	}

	private void menuExport2SubContent_Click(object sender, EventArgs e)
	{
		GetLvSelectedIndex();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int lvSelected in lvSelecteds)
		{
			string shareUrl = ShareHandler.GetShareUrl(config, lvSelected);
			if (!Utils.IsNullOrEmpty(shareUrl))
			{
				stringBuilder.Append(shareUrl);
				stringBuilder.AppendLine();
			}
		}
		if (stringBuilder.Length > 0)
		{
			Utils.SetClipboardData(Utils.Base64Encode(stringBuilder.ToString()));
			AppendText(notify: false, "节点订阅内容已复制到剪贴板");
		}
	}

	private void lvServers_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Control)
		{
			switch (e.KeyCode)
			{
			case Keys.A:
				menuSelectAll_Click(null, null);
				break;
			case Keys.C:
				menuExport2ShareUrl_Click(null, null);
				break;
			case Keys.V:
				menuAddServers_Click(null, null);
				break;
			case Keys.T:
				menuDownLoadServer_Click(null, null);
				break;
			case Keys.R:
				menuRealPingServer_Click(null, null);
				break;
			}
		}
		else if (e.KeyCode == Keys.Delete)
		{
			menuRemoveServer_Click(null, null);
		}
	}

	// 当前排序的列 key
	private string currentSortColumnKey = null;

	private void lvServers_ColumnClick(object sender, ColumnClickEventArgs e)
	{
		if (e.Column < 0 || e.Column >= lvServers.Columns.Count)
		{
			return;
		}
		try
		{
			// 获取被点击列的 key
			string columnKey = lvServers.Columns[e.Column].Tag as string;
			if (string.IsNullOrEmpty(columnKey))
			{
				return;
			}
			
			// 判断排序方向
			bool ascending;
			if (currentSortColumnKey == columnKey)
			{
				// 同一列，切换排序方向
				ascending = !currentSortAscending;
			}
			else
			{
				// 不同列，默认升序
				ascending = true;
			}
			
			// 根据 key 获取对应的枚举值
			EServerColName colName = GetServerColNameByKey(columnKey);
			if (ConfigHandler.SortServers(ref config, colName, ascending) != 0)
			{
				return;
			}
			RefreshServers();
			SetSortIconByKey(columnKey, ascending);
			currentSortColumnKey = columnKey;
			currentSortAscending = ascending;
			if (config?.uiItem != null)
			{
				config.uiItem.sortColumnKey = currentSortColumnKey;
				config.uiItem.sortAscending = currentSortAscending;
			}
		}
		catch
		{
		}
	}

	private EServerColName GetServerColNameByKey(string key)
	{
		return key switch
		{
			"def" => EServerColName.def,
			"configType" => EServerColName.configType,
			"remarks" => EServerColName.remarks,
			"address" => EServerColName.address,
			"port" => EServerColName.port,
			"security" => EServerColName.security,
			"network" => EServerColName.network,
			"tls" => EServerColName.tls,
			"subRemarks" => EServerColName.subRemarks,
			"minDelay" => EServerColName.minDelay,
			"avgDelay" => EServerColName.avgDelay,
			"testResult" => EServerColName.testResult,
			"MaxSpeed" => EServerColName.MaxSpeed,
			"lastTestTime" => EServerColName.lastTestTime,
			_ => EServerColName.def
		};
	}

	private void SetSortIconByKey(string columnKey, bool ascending)
	{
		try
		{
			IntPtr hWnd = SendMessage(lvServers.Handle, 4127u, IntPtr.Zero, IntPtr.Zero);
			
			// 清除所有列的排序图标
			for (int i = 0; i < lvServers.Columns.Count; i++)
			{
				HDITEM lParam = new HDITEM { mask = 4 };
				SendMessageHDITEM(hWnd, 4619u, (IntPtr)i, ref lParam);
				lParam.fmt &= -1537;
				SendMessageHDITEM(hWnd, 4620u, (IntPtr)i, ref lParam);
			}
			
			// 找到目标列的物理索引并设置排序图标
			for (int i = 0; i < lvServers.Columns.Count; i++)
			{
				if (lvServers.Columns[i].Tag as string == columnKey)
				{
					HDITEM lParam2 = new HDITEM { mask = 4 };
					SendMessageHDITEM(hWnd, 4619u, (IntPtr)i, ref lParam2);
					lParam2.fmt |= (ascending ? 1024 : 512);
					SendMessageHDITEM(hWnd, 4620u, (IntPtr)i, ref lParam2);
					break;
				}
			}
		}
		catch
		{
		}
	}

	private void tsbSubSetting_Click(object sender, EventArgs e)
	{
		new SubSettingForm().ShowDialog();
	}

	private void tsbSubUpdate_Click(object sender, EventArgs e)
	{
		UpdateSubscriptionProcess();
	}

	private void UpdateSubscriptionProcess()
	{
		new UpdateHandle().UpdateSubscriptionProcess(config, _updateUI);
		void _updateUI(bool success, string msg)
		{
			AppendText(notify: false, msg);
			if (success)
			{
				InitSubscriptionTabs();
				RefreshServers();
				ConfigHandler.ToJsonFile(config);
			}
		}
	}

	private void btnClearLog_Click(object sender, EventArgs e)
	{
		txtMsgBox.Clear();
	}

	private void btnClearResult_Click(object sender, EventArgs e)
	{
		int num = 0;
		foreach (ListViewItem item in lvServers.Items)
		{
			if (item.Tag != null)
			{
				int num2 = (int)item.Tag;
				if (num2 >= 0 && num2 < config.vmess.Count)
				{
					config.vmess[num2].minDelay = "";
					config.vmess[num2].avgDelay = "";
					config.vmess[num2].httpsDelay = "";
					config.vmess[num2].testResult = "";
					config.vmess[num2].MaxSpeed = "";
					config.vmess[num2].lastTestTime = "";
					num++;
				}
			}
			if (item.SubItems.ContainsKey("minDelay"))
				item.SubItems["minDelay"].Text = "";
			if (item.SubItems.ContainsKey("avgDelay"))
				item.SubItems["avgDelay"].Text = "";
			if (item.SubItems.ContainsKey("testResult"))
				item.SubItems["testResult"].Text = "";
			if (item.SubItems.ContainsKey("MaxSpeed"))
				item.SubItems["MaxSpeed"].Text = "";
			if (item.SubItems.ContainsKey("lastTestTime"))
			{
				item.SubItems["lastTestTime"].Text = "";
			}
		}
		ConfigHandler.ToJsonFile(config);
		AppendText(notify: false, $"已清空当前列表 {num} 个节点的测速结果");
	}

	private void btnTestDelay_Click(object sender, EventArgs e)
	{
		StartDelayTest(onlySelected: false);
	}

	private void btnTestSpeed_Click(object sender, EventArgs e)
	{
		StartDownloadTest(onlySelected: false);
	}

	private void tbDelayUrl_TextChanged(object sender, EventArgs e)
	{
		config.speedPingTestUrl = tbDelayUrl.Text.Trim();
	}

	private void tbPingCount_TextChanged(object sender, EventArgs e)
	{
		config.PingCount = tbPingCount.Text.Trim();
	}

	private void tbTimeout_TextChanged(object sender, EventArgs e)
	{
		config.Timeout = tbTimeout.Text.Trim();
	}

	private void tbSpeedUrl_TextChanged(object sender, EventArgs e)
	{
		config.speedTestUrl = tbSpeedUrl.Text.Trim();
	}

	private void tbDownloadThread_TextChanged(object sender, EventArgs e)
	{
		config.DownloadThread = tbDownloadThread.Text.Trim();
	}

	private void tbDownloadMaxTime_TextChanged(object sender, EventArgs e)
	{
		config.DownloadMaxTime = tbDownloadMaxTime.Text.Trim();
	}

	private void tbKeywordFilter_TextChanged(object sender, EventArgs e)
	{
		config.keywordFilter = tbKeywordFilter.Text;
		if (config.keywordFilterEnabled)
		{
			RefreshServersView();
		}
	}

	private void cbKeywordFilter_CheckedChanged(object sender, EventArgs e)
	{
		bool flag = cbKeywordFilter.Checked;
		config.keywordFilterEnabled = flag;
		tbKeywordFilter.Enabled = flag;
		RefreshServersView();
	}

	private void btnKeywordPreset_Click(object sender, EventArgs e)
	{
		UpdateKeywordPresetCheckStates();
		cmsKeywordPreset.Show(btnKeywordPreset, new Point(0, btnKeywordPreset.Height));
	}

	private void KeywordPreset_Click(object sender, EventArgs e)
	{
		if (!(sender is ToolStripMenuItem toolStripMenuItem))
		{
			return;
		}
		if (toolStripMenuItem.Name == "CLEAR")
		{
			tbKeywordFilter.Text = string.Empty;
			UpdateKeywordPresetCheckStates();
		}
		else
		{
			if (!(toolStripMenuItem.Tag is string text) || string.IsNullOrEmpty(text))
			{
				return;
			}
			string text2 = tbKeywordFilter.Text?.Trim() ?? string.Empty;
			List<string> keywords = new List<string>();
			if (!string.IsNullOrEmpty(text2))
			{
				keywords.AddRange(text2.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
			}
			string[] array = text.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.All((string pk) => keywords.Contains(pk)))
			{
				string[] array2 = array;
				foreach (string item in array2)
				{
					keywords.Remove(item);
				}
			}
			else
			{
				string[] array2 = array;
				foreach (string item2 in array2)
				{
					if (!keywords.Contains(item2))
					{
						keywords.Add(item2);
					}
				}
			}
			tbKeywordFilter.Text = string.Join(",", keywords);
			UpdateKeywordPresetCheckStates();
			cmsKeywordPreset.Show(btnKeywordPreset, new Point(0, btnKeywordPreset.Height));
		}
	}

	private void UpdateKeywordPresetCheckStates()
	{
		string text = tbKeywordFilter.Text?.Trim() ?? string.Empty;
		List<string> keywords = new List<string>();
		if (!string.IsNullOrEmpty(text))
		{
			keywords.AddRange(text.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
		}
		foreach (ToolStripItem item in cmsKeywordPreset.Items)
		{
			if (item is ToolStripMenuItem toolStripMenuItem && item.Tag is string text2 && !string.IsNullOrEmpty(text2))
			{
				string[] source = text2.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				toolStripMenuItem.Checked = source.All((string pk) => keywords.Contains(pk));
			}
		}
	}

	private ToolStripMenuItem CreatePresetMenuItem(string displayText, string keyword, string name = null)
	{
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(displayText);
		toolStripMenuItem.Tag = keyword;
		toolStripMenuItem.Name = name ?? string.Empty;
		toolStripMenuItem.Click += KeywordPreset_Click;
		return toolStripMenuItem;
	}

	public void InitSubscriptionTabs()
	{
		InitSubscriptionTabs(restoreSelection: true);
	}

	public void InitSubscriptionTabs(bool restoreSelection)
	{
		if (tabSubscriptions == null)
		{
			return;
		}
		string text = null;
		if (restoreSelection && tabSubscriptions.SelectedTab != null)
		{
			text = tabSubscriptions.SelectedTab.Tag as string;
		}
		tabSubscriptions.TabPages.Clear();
		TabPage tabPage = new TabPage("所有节点");
		tabPage.Tag = null;
		tabSubscriptions.TabPages.Add(tabPage);
		if (config.vmess != null && config.vmess.Any((VmessItem v) => string.IsNullOrEmpty(v.subid)))
		{
			TabPage tabPage2 = new TabPage("无分组");
			tabPage2.Tag = "__unassigned__";
			tabSubscriptions.TabPages.Add(tabPage2);
		}
		if (config.subItem != null)
		{
			foreach (SubItem item in config.subItem)
			{
				if (item.enabled)
				{
					TabPage tabPage3 = new TabPage(item.remarks);
					tabPage3.Tag = item.id;
					tabSubscriptions.TabPages.Add(tabPage3);
				}
			}
		}
		if (tabSubscriptions.TabPages.Count <= 0)
		{
			return;
		}
		if (restoreSelection && text != null)
		{
			bool flag = false;
			for (int num = 0; num < tabSubscriptions.TabPages.Count; num++)
			{
				if (tabSubscriptions.TabPages[num].Tag as string == text)
				{
					tabSubscriptions.SelectedIndex = num;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				tabSubscriptions.SelectedIndex = 0;
			}
		}
		else if (restoreSelection && text == null)
		{
			tabSubscriptions.SelectedIndex = 0;
		}
		else
		{
			tabSubscriptions.SelectedIndex = 0;
		}
	}

	private void tabSubscriptions_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (tabSubscriptions.SelectedTab != null)
		{
			currentSubFilter = tabSubscriptions.SelectedTab.Tag as string;
			if (config?.uiItem != null)
			{
				config.uiItem.selectedSubId = currentSubFilter ?? "__all__";
			}
			RefreshServersView();
		}
	}

	private void UpdateUnassignedTabVisibility()
	{
		if (tabSubscriptions == null)
		{
			return;
		}
		bool flag = config.vmess != null && config.vmess.Any((VmessItem v) => string.IsNullOrEmpty(v.subid));
		TabPage tabPage = null;
		for (int num = 0; num < tabSubscriptions.TabPages.Count; num++)
		{
			if (tabSubscriptions.TabPages[num].Tag as string == "__unassigned__")
			{
				tabPage = tabSubscriptions.TabPages[num];
				break;
			}
		}
		if (flag && tabPage == null)
		{
			TabPage tabPage2 = new TabPage("无分组");
			tabPage2.Tag = "__unassigned__";
			tabSubscriptions.TabPages.Insert(1, tabPage2);
		}
		else if (!flag && tabPage != null)
		{
			if (tabSubscriptions.SelectedTab == tabPage)
			{
				tabSubscriptions.SelectedIndex = 0;
			}
			tabSubscriptions.TabPages.Remove(tabPage);
		}
	}

	private void lvServers_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
	{
		try
		{
			EServerColName colName = GetServerColNameByIndex(e.ColumnIndex);
			string key = colName.ToString();
			
			// 如果列被隐藏，不保存宽度变化
			if (!IsColumnVisible(key))
				return;
			
			int columnHeaderMinWidth = GetColumnHeaderMinWidth(e.ColumnIndex);
			if (columnHeaderMinWidth > 0 && lvServers.Columns[e.ColumnIndex].Width < columnHeaderMinWidth)
			{
				lvServers.Columns[e.ColumnIndex].Width = columnHeaderMinWidth;
			}
			if (config.uiItem?.mainLvColWidth != null)
			{
				if (config.uiItem.mainLvColWidth.ContainsKey(key))
				{
					config.uiItem.mainLvColWidth[key] = lvServers.Columns[e.ColumnIndex].Width;
				}
			}
		}
		catch
		{
		}
	}

	private void lvServers_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
	{
		// 如果列当前是隐藏的（宽度为0），不允许用户调整
		EServerColName colName = GetServerColNameByIndex(e.ColumnIndex);
		if (!IsColumnVisible(colName.ToString()))
		{
			e.NewWidth = 0;
			e.Cancel = true;
			return;
		}
		
		if (e.NewWidth < 0)
		{
			int num = CalculateAutoColumnWidth(e.ColumnIndex);
			if (num > 0)
			{
				lvServers.Columns[e.ColumnIndex].Width = num;
				e.Cancel = true;
			}
		}
		else
		{
			int columnHeaderMinWidth = GetColumnHeaderMinWidth(e.ColumnIndex);
			if (columnHeaderMinWidth > 0 && e.NewWidth < columnHeaderMinWidth)
			{
				lvServers.Columns[e.ColumnIndex].Width = columnHeaderMinWidth;
				e.Cancel = true;
			}
		}
	}

	private void tssTool_Click(object sender, EventArgs e)
	{
		AboutForm aboutForm = new AboutForm();
		aboutForm.Owner = this;
		aboutForm.ShowDialog();
	}

	private void toolStripButton1_Click(object sender, EventArgs e)
	{
		Process.Start("https://bulianglin.com/archives/nodescatch.html");
	}

	private void menuExport2Base64_Click(object sender, EventArgs e)
	{
		GetLvSelectedIndex();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int lvSelected in lvSelecteds)
		{
			string shareUrl = ShareHandler.GetShareUrl(config, lvSelected);
			if (!Utils.IsNullOrEmpty(shareUrl))
			{
				stringBuilder.Append(shareUrl);
				stringBuilder.AppendLine();
			}
		}
		if (stringBuilder.Length > 0)
		{
			string path = Utils.ShowSaveFileDialog("文本文档（*.txt）|*.txt");
			if (!Utils.IsNullOrEmpty(path))
			{
				File.WriteAllText(path, Utils.Base64Encode(stringBuilder.ToString()), Encoding.UTF8);
				AppendText(notify: false, "保存成功");
			}
		}
	}

	private void menuExport2Clash_Click(object sender, EventArgs e)
	{
		GetLvSelectedIndex();
		if (!ConfigHandler.TcpClientCheck("127.0.0.1", 25500))
		{
			MessageBox.Show($"无法连接到Subconverter(端口:{25500})，请确认是否启动！建议重新打开测速软件");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		List<VmessItem> listVlessReality = new List<VmessItem>();
		List<VmessItem> listAnyTLS = new List<VmessItem>();
		foreach (int lvSelected in lvSelecteds)
		{
			if (lvSelected >= 0 && lvSelected < config.vmess.Count)
			{
				VmessItem vmessItem = config.vmess[lvSelected];
				// VLESS 节点全部单独处理（绕过 subconverter）
				if (vmessItem.configType == 5)
				{
					bool isBrokenReality = vmessItem.streamSecurity == "reality" && 
						(string.IsNullOrEmpty(vmessItem.publicKey) || string.IsNullOrEmpty(vmessItem.shortId));
					if (!isBrokenReality)
					{
						listVlessReality.Add(vmessItem);
					}
					continue;
				}
				// AnyTLS 节点单独处理
				if (vmessItem.configType == 12)
				{
					listAnyTLS.Add(vmessItem);
					continue;
				}
			}
			string shareUrl = ShareHandler.GetShareUrl(config, lvSelected);
			if (!Utils.IsNullOrEmpty(shareUrl))
			{
				stringBuilder.Append(shareUrl);
				stringBuilder.AppendLine();
			}
		}
		if (stringBuilder.Length <= 0 && listVlessReality.Count == 0 && listAnyTLS.Count == 0)
		{
			return;
		}
		string text = Guid.NewGuid().ToString("N");
		string path = Utils.GetPath("subconverter\\temp_" + text + ".txt");
		try
		{
			string path2 = Utils.ShowSaveFileDialog("Mihomo配置文件（*.yaml）|*.yaml");
			if (Utils.IsNullOrEmpty(path2))
			{
				return;
			}
			
			string yamlContent = "";
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Insert(0, "ss://YWVzLTI1Ni1nY206ZmFCQW9ENTRrODdVSkc3QDEuMS4xLjE6NjY2#%e5%8d%a0%e4%bd%8d%e8%8a%82%e7%82%b9" + Environment.NewLine);
				File.WriteAllText(path, Utils.Base64Encode(stringBuilder.ToString()), Encoding.UTF8);
				HttpWebRequest obj = (HttpWebRequest)WebRequest.Create($"http://127.0.0.1:{25500}/sub?target=clash&url=temp_{text}.txt&insert=false");
				obj.Method = "GET";
				obj.Timeout = 10000;
				obj.ReadWriteTimeout = 10000;
				obj.ContinueTimeout = 10000;
				using StreamReader streamReader = new StreamReader(obj.GetResponse().GetResponseStream(), Encoding.UTF8);
				yamlContent = streamReader.ReadToEnd();
			}
			else
			{
				yamlContent = "mixed-port: 7890\r\nmode: Global\r\nlog-level: info\r\nexternal-controller: 127.0.0.1:9090\r\nproxies:\r\nproxy-groups:\r\n  - name: PROXY\r\n    type: select\r\n    proxies:\r\nrules:\r\n  - MATCH,PROXY\r\n";
			}
			
			// 插入 VLESS Reality 节点
			if (listVlessReality.Count > 0)
			{
				yamlContent = InsertVlessRealityToYaml(yamlContent, listVlessReality);
			}
			
			// 插入 AnyTLS 节点
			if (listAnyTLS.Count > 0)
			{
				yamlContent = InsertAnyTLSToYaml(yamlContent, listAnyTLS);
			}
			
			yamlContent = EnsureUnifiedDelayInYaml(yamlContent);
			File.WriteAllText(path2, yamlContent, Encoding.UTF8);
			AppendText(notify: false, "保存文件成功");
		}
		catch (Exception ex)
		{
			MessageBox.Show("导出配置文件失败！返回异常：" + ex.Message);
		}
		finally
		{
			try
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch
			{
			}
		}
	}

	/// <summary>
	/// 生成单个 VLESS 节点的 Mihomo YAML 配置（支持 TCP/WS/gRPC/H2 + TLS/Reality/None）
	/// </summary>
	private static void AppendVlessNodeYaml(StringBuilder sbProxies, StringBuilder sbProxyNames, VmessItem item, string proxyName)
	{
		string network = string.IsNullOrEmpty(item.network) ? "tcp" : item.network.ToLower();
		bool isReality = item.streamSecurity == "reality" || !string.IsNullOrEmpty(item.publicKey);
		bool isTls = isReality || item.streamSecurity == "tls" || item.streamSecurity == "xtls";
		bool skipCertVerify = item.allowInsecure == "true" || item.allowInsecure == "1";

		sbProxies.AppendLine("  - name: \"" + proxyName + "\"");
		sbProxies.AppendLine("    type: vless");
		sbProxies.AppendLine("    server: " + item.address);
		sbProxies.AppendLine($"    port: {item.port}");
		sbProxies.AppendLine("    uuid: \"" + item.id + "\"");
		sbProxies.AppendLine("    network: " + network);
		sbProxies.AppendLine("    tls: " + isTls.ToString().ToLower());
		sbProxies.AppendLine("    udp: true");

		if (!string.IsNullOrEmpty(item.flow))
		{
			sbProxies.AppendLine("    flow: " + item.flow);
		}

		if (isTls)
		{
			string fingerprint = string.IsNullOrEmpty(item.fingerprint) ? "chrome" : item.fingerprint;
			sbProxies.AppendLine("    client-fingerprint: " + fingerprint);

			string servername = !string.IsNullOrEmpty(item.sni) ? item.sni : item.requestHost;
			if (!string.IsNullOrEmpty(servername))
			{
				sbProxies.AppendLine("    servername: " + servername);
			}

			sbProxies.AppendLine("    skip-cert-verify: " + skipCertVerify.ToString().ToLower());
		}

		if (isReality)
		{
			sbProxies.AppendLine("    reality-opts:");
			if (!string.IsNullOrEmpty(item.publicKey))
			{
				sbProxies.AppendLine("      public-key: " + item.publicKey);
			}
			if (!string.IsNullOrEmpty(item.shortId))
			{
				sbProxies.AppendLine("      short-id: \"" + item.shortId + "\"");
			}
		}

		if (network == "ws")
		{
			sbProxies.AppendLine("    ws-opts:");
			string wsPath = string.IsNullOrEmpty(item.path) ? "/" : item.path;
			sbProxies.AppendLine("      path: \"" + wsPath + "\"");
			string wsHost = !string.IsNullOrEmpty(item.requestHost) ? item.requestHost : item.sni;
			if (!string.IsNullOrEmpty(wsHost))
			{
				sbProxies.AppendLine("      headers:");
				sbProxies.AppendLine("        Host: \"" + wsHost + "\"");
			}
		}
		else if (network == "grpc")
		{
			if (!string.IsNullOrEmpty(item.path))
			{
				sbProxies.AppendLine("    grpc-opts:");
				sbProxies.AppendLine("      grpc-service-name: \"" + item.path + "\"");
			}
		}
		else if (network == "h2" || network == "http")
		{
			sbProxies.AppendLine("    h2-opts:");
			if (!string.IsNullOrEmpty(item.path))
			{
				sbProxies.AppendLine("      path:");
				sbProxies.AppendLine("        - \"" + item.path + "\"");
			}
		}

		sbProxyNames.AppendLine("      - \"" + proxyName + "\"");
	}

	/// <summary>
	/// 生成 VLESS 节点的 Mihomo YAML 配置并插入到现有配置中
	/// </summary>
	private string InsertVlessRealityToYaml(string yamlContent, List<VmessItem> listVlessReality)
	{
		if (yamlContent.Contains("proxies: ~"))
		{
			yamlContent = yamlContent.Replace("proxies: ~", "proxies:");
		}
		StringBuilder sbProxies = new StringBuilder();
		StringBuilder sbProxyNames = new StringBuilder();
		
		for (int i = 0; i < listVlessReality.Count; i++)
		{
			VmessItem item = listVlessReality[i];
			string proxyName = string.IsNullOrEmpty(item.remarks) ? $"vless-{i}" : item.remarks;
			AppendVlessNodeYaml(sbProxies, sbProxyNames, item, proxyName);
		}
		
		int num = yamlContent.IndexOf("proxies:");
		if (num >= 0)
		{
			int startIndex = yamlContent.IndexOf("\n", num) + 1;
			yamlContent = yamlContent.Insert(startIndex, sbProxies.ToString());
		}
		int num2 = yamlContent.IndexOf("proxy-groups:");
		if (num2 >= 0)
		{
			int num3 = yamlContent.IndexOf("proxies:", num2);
			if (num3 >= 0)
			{
				int startIndex2 = yamlContent.IndexOf("\n", num3) + 1;
				yamlContent = yamlContent.Insert(startIndex2, sbProxyNames.ToString());
			}
		}
		
		return yamlContent;
	}
	
	/// <summary>
	/// 生成 AnyTLS 节点的 Mihomo YAML 配置并插入到现有配置中
	/// </summary>
	private string InsertAnyTLSToYaml(string yamlContent, List<VmessItem> listAnyTLS)
	{
		StringBuilder sbProxies = new StringBuilder();
		StringBuilder sbProxyNames = new StringBuilder();
		
		for (int i = 0; i < listAnyTLS.Count; i++)
		{
			VmessItem item = listAnyTLS[i];
			string proxyName = string.IsNullOrEmpty(item.remarks) ? $"anytls-{i}" : item.remarks;
			bool skipCertVerify = item.allowInsecure == "true" || item.allowInsecure == "1";
			
			sbProxies.AppendLine("  - name: \"" + proxyName + "\"");
			sbProxies.AppendLine("    type: anytls");
			sbProxies.AppendLine("    server: " + item.address);
			sbProxies.AppendLine($"    port: {item.port}");
			sbProxies.AppendLine("    password: \"" + item.id + "\"");
			if (!string.IsNullOrEmpty(item.sni))
			{
				sbProxies.AppendLine("    sni: " + item.sni);
			}
			sbProxies.AppendLine("    skip-cert-verify: " + skipCertVerify.ToString().ToLower());
			sbProxies.AppendLine("    udp: true");
			
			sbProxyNames.AppendLine("      - \"" + proxyName + "\"");
		}
		
		int num = yamlContent.IndexOf("proxies:");
		if (num >= 0)
		{
			int startIndex = yamlContent.IndexOf("\n", num) + 1;
			yamlContent = yamlContent.Insert(startIndex, sbProxies.ToString());
		}
		int num2 = yamlContent.IndexOf("proxy-groups:");
		if (num2 >= 0)
		{
			int num3 = yamlContent.IndexOf("proxies:", num2);
			if (num3 >= 0)
			{
				int startIndex2 = yamlContent.IndexOf("\n", num3) + 1;
				yamlContent = yamlContent.Insert(startIndex2, sbProxyNames.ToString());
			}
		}
		
		return yamlContent;
	}

	private void SortListView()
	{
		if (string.IsNullOrEmpty(currentSortColumnKey))
		{
			return;
		}
		EServerColName name = GetServerColNameByKey(currentSortColumnKey);
		if (name == EServerColName.def)
		{
			return;
		}
		try
		{
			if (ConfigHandler.SortServers(ref config, name, currentSortAscending) == 0)
			{
				RefreshServers();
				SetSortIconByKey(currentSortColumnKey, currentSortAscending);
			}
		}
		catch (Exception ex)
		{
			AppendText(notify: false, "排序失败：" + ex.Message);
		}
	}

	private void MainForm_DragEnter(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			e.Effect = DragDropEffects.All;
		}
		else
		{
			e.Effect = DragDropEffects.None;
		}
	}

	private void MainForm_DragDrop(object sender, DragEventArgs e)
	{
		string[] obj = (string[])e.Data.GetData(DataFormats.FileDrop);
		int num = 0;
		string[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			string clipboardData = File.ReadAllText(array[i], Encoding.UTF8);
			num += MainFormHandler.Instance.AddBatchServers(config, clipboardData);
		}
		if (num > 0)
		{
			RefreshServers();
			ConfigHandler.ToJsonFile(config);
			AppendText(notify: false, $"成功从文件中导入{num}个节点");
		}
		else
		{
			UI.Show("未能成功解析节点，请检查文件内容！", this);
		}
	}

	private void button1_Click_1(object sender, EventArgs e)
	{
		new SubSettingForm().ShowDialog();
	}

	private void 导入到节点列表ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		UpdateSubscriptionProcess();
	}

	private void 订阅列表ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		new SubSettingForm().ShowDialog();
	}

	private void button2_Click(object sender, EventArgs e)
	{
		AboutForm aboutForm = new AboutForm();
		aboutForm.Owner = this;
		aboutForm.ShowDialog();
	}

	private void MainForm_ResizeEnd(object sender, EventArgs e)
	{
		if (base.WindowState == FormWindowState.Normal)
		{
			config.uiItem.mainSize = base.Size;
			config.uiItem.mainLocation = base.Location;
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		return base.ProcessCmdKey(ref msg, keyData);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			// 释放列头 NativeWindow
			if (columnHeaderNativeWindow != null)
			{
				columnHeaderNativeWindow.ReleaseHandle();
				columnHeaderNativeWindow = null;
			}
			
			// 释放列头右键菜单
			if (cmsColumnHeader != null)
			{
				cmsColumnHeader.Dispose();
				cmsColumnHeader = null;
			}
			
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.lvServers = new nodesCatchNext.Base.ListViewFlickerFree();
		this.cmsLv = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.menuRealPingServer = new System.Windows.Forms.ToolStripMenuItem();
		this.menuDownLoadServer = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.menuAddServers = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.menuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
		this.menuEditServer = new System.Windows.Forms.ToolStripMenuItem();
		this.menuRemoveServer = new System.Windows.Forms.ToolStripMenuItem();
		this.menuRemoveDuplicateServer = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.menuExport2ShareUrl = new System.Windows.Forms.ToolStripMenuItem();
		this.menuExport2SubContent = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
		this.menuExport2Base64 = new System.Windows.Forms.ToolStripMenuItem();
		this.menuExport2Clash = new System.Windows.Forms.ToolStripMenuItem();
		this.panel2 = new System.Windows.Forms.Panel();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.txtMsgBox = new System.Windows.Forms.TextBox();
		this.panel1 = new System.Windows.Forms.Panel();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.groupBox5 = new System.Windows.Forms.Panel();
		this.lblDelayUrl = new System.Windows.Forms.Label();
		this.tbDelayUrl = new System.Windows.Forms.TextBox();
		this.lblPingCount = new System.Windows.Forms.Label();
		this.tbPingCount = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.tbTimeout = new System.Windows.Forms.TextBox();
		this.btnTestDelay = new System.Windows.Forms.Button();
		this.lblSpeedUrl = new System.Windows.Forms.Label();
		this.tbSpeedUrl = new System.Windows.Forms.TextBox();
		this.lblDownloadThread = new System.Windows.Forms.Label();
		this.tbDownloadThread = new System.Windows.Forms.TextBox();
		this.lblDownloadMaxTime = new System.Windows.Forms.Label();
		this.tbDownloadMaxTime = new System.Windows.Forms.TextBox();
		this.btnTestSpeed = new System.Windows.Forms.Button();
		this.btnClearResult = new System.Windows.Forms.Button();
		this.btnClearLog = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.cbKeywordFilter = new System.Windows.Forms.CheckBox();
		this.tbKeywordFilter = new System.Windows.Forms.TextBox();
		this.btnKeywordPreset = new System.Windows.Forms.Button();
		this.button1 = new System.Windows.Forms.Button();
		this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.订阅列表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.导入到节点列表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.cmsKeywordPreset = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.toolTipKeywordFilter = new System.Windows.Forms.ToolTip(this.components);
		this.cmsMain = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		this.cmsLv.SuspendLayout();
		this.panel2.SuspendLayout();
		this.groupBox1.SuspendLayout();
		this.panel1.SuspendLayout();
		this.groupBox3.SuspendLayout();
		this.groupBox5.SuspendLayout();
		this.contextMenuStrip1.SuspendLayout();
		this.cmsMain.SuspendLayout();
		base.SuspendLayout();
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 0);
		this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer1.Panel1.Controls.Add(this.lvServers);
		this.splitContainer1.Panel1MinSize = 0;
		this.splitContainer1.Panel2.Controls.Add(this.panel2);
		this.splitContainer1.Panel2.Controls.Add(this.panel1);
		this.splitContainer1.Panel2MinSize = 0;
		this.splitContainer1.Size = new System.Drawing.Size(1841, 1283);
		this.splitContainer1.SplitterDistance = 774;
		this.splitContainer1.SplitterWidth = 5;
		this.splitContainer1.TabIndex = 2;
		this.splitContainer1.TabStop = false;
		this.lvServers.ContextMenuStrip = this.cmsLv;
		this.lvServers.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lvServers.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lvServers.FullRowSelect = true;
		this.lvServers.GridLines = true;
		this.lvServers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
		this.lvServers.HideSelection = false;
		this.lvServers.Location = new System.Drawing.Point(0, 0);
		this.lvServers.Margin = new System.Windows.Forms.Padding(4);
		this.lvServers.MultiSelect = false;
		this.lvServers.Name = "lvServers";
		this.lvServers.Size = new System.Drawing.Size(1841, 774);
		this.lvServers.TabIndex = 0;
		this.lvServers.UseCompatibleStateImageBehavior = false;
		this.lvServers.View = System.Windows.Forms.View.Details;
		this.lvServers.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(lvServers_ColumnClick);
		this.lvServers.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(lvServers_ColumnWidthChanged);
		this.lvServers.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(lvServers_ColumnWidthChanging);
		this.lvServers.DoubleClick += new System.EventHandler(lvServers_DoubleClick);
		this.lvServers.KeyDown += new System.Windows.Forms.KeyEventHandler(lvServers_KeyDown);
		this.cmsLv.ImageScalingSize = new System.Drawing.Size(20, 20);
		this.cmsLv.Items.AddRange(new System.Windows.Forms.ToolStripItem[15]
		{
			this.menuRealPingServer, this.menuDownLoadServer, this.toolStripSeparator3, this.menuAddServers, this.toolStripSeparator4, this.menuSelectAll, this.menuEditServer,
			this.menuRemoveServer, this.menuRemoveDuplicateServer, this.toolStripSeparator5, this.menuExport2ShareUrl, this.menuExport2SubContent, this.toolStripSeparator6, this.menuExport2Base64,
			this.menuExport2Clash
		});
		this.cmsLv.Name = "cmsLv";
		this.cmsLv.Size = new System.Drawing.Size(285, 340);
		this.menuRealPingServer.Name = "menuRealPingServer";
		this.menuRealPingServer.Size = new System.Drawing.Size(284, 24);
		this.menuRealPingServer.Text = "测试延迟(Ctrl+R)";
		this.menuRealPingServer.Click += new System.EventHandler(menuRealPingServer_Click);
		this.menuDownLoadServer.Name = "menuDownLoadServer";
		this.menuDownLoadServer.Size = new System.Drawing.Size(284, 24);
		this.menuDownLoadServer.Text = "测试下载(Ctrl+T)";
		this.menuDownLoadServer.Click += new System.EventHandler(menuDownLoadServer_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(281, 6);
		this.menuAddServers.Name = "menuAddServers";
		this.menuAddServers.Size = new System.Drawing.Size(284, 24);
		this.menuAddServers.Text = "从剪贴板导入节点(Ctrl+V)";
		this.menuAddServers.Click += new System.EventHandler(menuAddServers_Click);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(281, 6);
		this.menuSelectAll.Name = "menuSelectAll";
		this.menuSelectAll.Size = new System.Drawing.Size(284, 24);
		this.menuSelectAll.Text = "全选(Ctrl+A)";
		this.menuSelectAll.Click += new System.EventHandler(menuSelectAll_Click);
		this.menuEditServer.Name = "menuEditServer";
		this.menuEditServer.Size = new System.Drawing.Size(284, 24);
		this.menuEditServer.Text = "编辑节点(双击)";
		this.menuEditServer.Click += new System.EventHandler(menuEditServer_Click);
		this.menuRemoveServer.Name = "menuRemoveServer";
		this.menuRemoveServer.Size = new System.Drawing.Size(284, 24);
		this.menuRemoveServer.Text = "删除选中节点(多选)(Delete)";
		this.menuRemoveServer.Click += new System.EventHandler(menuRemoveServer_Click);
		this.menuRemoveDuplicateServer.Name = "menuRemoveDuplicateServer";
		this.menuRemoveDuplicateServer.Size = new System.Drawing.Size(284, 24);
		this.menuRemoveDuplicateServer.Text = "移除重复节点";
		this.menuRemoveDuplicateServer.Click += new System.EventHandler(menuRemoveDuplicateServer_Click);
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(281, 6);
		this.menuExport2ShareUrl.Name = "menuExport2ShareUrl";
		this.menuExport2ShareUrl.Size = new System.Drawing.Size(284, 24);
		this.menuExport2ShareUrl.Text = "导出分享URL到剪贴板(Ctrl+C)";
		this.menuExport2ShareUrl.Click += new System.EventHandler(menuExport2ShareUrl_Click);
		this.menuExport2SubContent.Name = "menuExport2SubContent";
		this.menuExport2SubContent.Size = new System.Drawing.Size(284, 24);
		this.menuExport2SubContent.Text = "导出订阅内容到剪贴板";
		this.menuExport2SubContent.Click += new System.EventHandler(menuExport2SubContent_Click);
		this.toolStripSeparator6.Name = "toolStripSeparator6";
		this.toolStripSeparator6.Size = new System.Drawing.Size(281, 6);
		this.menuExport2Base64.Name = "menuExport2Base64";
		this.menuExport2Base64.Size = new System.Drawing.Size(284, 24);
		this.menuExport2Base64.Text = "导出Base64通用订阅文件";
		this.menuExport2Base64.Click += new System.EventHandler(menuExport2Base64_Click);
		this.menuExport2Clash.Name = "menuExport2Clash";
		this.menuExport2Clash.Size = new System.Drawing.Size(284, 24);
		this.menuExport2Clash.Text = "导出Mihomo订阅文件";
		this.menuExport2Clash.Click += new System.EventHandler(menuExport2Clash_Click);
		this.panel2.Controls.Add(this.groupBox1);
		this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel2.Location = new System.Drawing.Point(0, 194);
		this.panel2.Margin = new System.Windows.Forms.Padding(4);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(1841, 310);
		this.panel2.TabIndex = 1;
		this.groupBox1.Controls.Add(this.txtMsgBox);
		this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBox1.Location = new System.Drawing.Point(0, 0);
		this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.groupBox1.Size = new System.Drawing.Size(1841, 310);
		this.groupBox1.TabIndex = 12;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "反馈";
		this.txtMsgBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.txtMsgBox.BackColor = System.Drawing.SystemColors.ButtonShadow;
		this.txtMsgBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtMsgBox.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtMsgBox.ForeColor = System.Drawing.SystemColors.Info;
		this.txtMsgBox.Location = new System.Drawing.Point(3, 20);
		this.txtMsgBox.Margin = new System.Windows.Forms.Padding(4);
		this.txtMsgBox.Multiline = true;
		this.txtMsgBox.Name = "txtMsgBox";
		this.txtMsgBox.ReadOnly = true;
		this.txtMsgBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.txtMsgBox.Size = new System.Drawing.Size(1834, 288);
		this.txtMsgBox.TabIndex = 5;
		this.panel1.Controls.Add(this.groupBox3);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
		this.panel1.Location = new System.Drawing.Point(0, 0);
		this.panel1.Margin = new System.Windows.Forms.Padding(4);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(1841, 194);
		this.panel1.TabIndex = 0;
		this.groupBox3.Controls.Add(this.groupBox5);
		this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBox3.Location = new System.Drawing.Point(0, 0);
		this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.groupBox3.Size = new System.Drawing.Size(1841, 194);
		this.groupBox3.TabIndex = 0;
		this.groupBox3.TabStop = false;
		this.groupBox5.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.groupBox5.Controls.Add(this.lblDelayUrl);
		this.groupBox5.Controls.Add(this.tbDelayUrl);
		this.groupBox5.Controls.Add(this.lblPingCount);
		this.groupBox5.Controls.Add(this.tbPingCount);
		this.groupBox5.Controls.Add(this.label4);
		this.groupBox5.Controls.Add(this.tbTimeout);
		this.groupBox5.Controls.Add(this.btnTestDelay);
		this.groupBox5.Controls.Add(this.lblSpeedUrl);
		this.groupBox5.Controls.Add(this.tbSpeedUrl);
		this.groupBox5.Controls.Add(this.lblDownloadThread);
		this.groupBox5.Controls.Add(this.tbDownloadThread);
		this.groupBox5.Controls.Add(this.lblDownloadMaxTime);
		this.groupBox5.Controls.Add(this.tbDownloadMaxTime);
		this.groupBox5.Controls.Add(this.btnTestSpeed);
		this.groupBox5.Controls.Add(this.btnClearResult);
		this.groupBox5.Controls.Add(this.btnClearLog);
		this.groupBox5.Controls.Add(this.button2);
		this.groupBox5.Controls.Add(this.cbKeywordFilter);
		this.groupBox5.Controls.Add(this.tbKeywordFilter);
		this.groupBox5.Controls.Add(this.btnKeywordPreset);
		this.groupBox5.Controls.Add(this.button1);
		this.groupBox5.Location = new System.Drawing.Point(0, -8);
		this.groupBox5.Margin = new System.Windows.Forms.Padding(4);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Padding = new System.Windows.Forms.Padding(4);
		this.groupBox5.Size = new System.Drawing.Size(1841, 202);
		this.groupBox5.TabIndex = 29;
		// 延迟测试配置行（第一行 Y=28）
		this.lblDelayUrl.AutoSize = true;
		this.lblDelayUrl.Location = new System.Drawing.Point(24, 32);
		this.lblDelayUrl.Name = "lblDelayUrl";
		this.lblDelayUrl.Size = new System.Drawing.Size(76, 15);
		this.lblDelayUrl.TabIndex = 1;
		this.lblDelayUrl.Text = "延迟URL：";
		this.tbDelayUrl.Location = new System.Drawing.Point(104, 28);
		this.tbDelayUrl.Name = "tbDelayUrl";
		this.tbDelayUrl.Size = new System.Drawing.Size(420, 25);
		this.tbDelayUrl.TabIndex = 2;
		this.tbDelayUrl.TextChanged += new System.EventHandler(tbDelayUrl_TextChanged);
		this.lblPingCount.AutoSize = true;
		this.lblPingCount.Location = new System.Drawing.Point(540, 32);
		this.lblPingCount.Name = "lblPingCount";
		this.lblPingCount.Size = new System.Drawing.Size(82, 15);
		this.lblPingCount.TabIndex = 3;
		this.lblPingCount.Text = "测试次数：";
		this.tbPingCount.Location = new System.Drawing.Point(625, 28);
		this.tbPingCount.Name = "tbPingCount";
		this.tbPingCount.Size = new System.Drawing.Size(55, 25);
		this.tbPingCount.TabIndex = 4;
		this.tbPingCount.Text = "3";
		this.tbPingCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tbPingCount.TextChanged += new System.EventHandler(tbPingCount_TextChanged);
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(696, 32);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(113, 15);
		this.label4.TabIndex = 5;
		this.label4.Text = "延迟超时(秒)：";
		this.tbTimeout.Location = new System.Drawing.Point(810, 28);
		this.tbTimeout.Name = "tbTimeout";
		this.tbTimeout.Size = new System.Drawing.Size(55, 25);
		this.tbTimeout.TabIndex = 6;
		this.tbTimeout.Text = "5";
		this.tbTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tbTimeout.TextChanged += new System.EventHandler(tbTimeout_TextChanged);
		this.btnTestDelay.Location = new System.Drawing.Point(885, 20);
		this.btnTestDelay.Name = "btnTestDelay";
		this.btnTestDelay.Size = new System.Drawing.Size(130, 38);
		this.btnTestDelay.TabIndex = 7;
		this.btnTestDelay.Text = "测试延迟";
		this.btnTestDelay.UseVisualStyleBackColor = true;
		this.btnTestDelay.Click += new System.EventHandler(btnTestDelay_Click);
		// 下载测试配置行（第二行 Y=82）
		this.lblSpeedUrl.AutoSize = true;
		this.lblSpeedUrl.Location = new System.Drawing.Point(24, 86);
		this.lblSpeedUrl.Name = "lblSpeedUrl";
		this.lblSpeedUrl.Size = new System.Drawing.Size(76, 15);
		this.lblSpeedUrl.TabIndex = 8;
		this.lblSpeedUrl.Text = "下载URL：";
		this.tbSpeedUrl.Location = new System.Drawing.Point(104, 82);
		this.tbSpeedUrl.Name = "tbSpeedUrl";
		this.tbSpeedUrl.Size = new System.Drawing.Size(420, 25);
		this.tbSpeedUrl.TabIndex = 9;
		this.tbSpeedUrl.TextChanged += new System.EventHandler(tbSpeedUrl_TextChanged);
		this.lblDownloadThread.AutoSize = true;
		this.lblDownloadThread.Location = new System.Drawing.Point(540, 86);
		this.lblDownloadThread.Name = "lblDownloadThread";
		this.lblDownloadThread.Size = new System.Drawing.Size(82, 15);
		this.lblDownloadThread.TabIndex = 10;
		this.lblDownloadThread.Text = "测试线程：";
		this.tbDownloadThread.Location = new System.Drawing.Point(625, 82);
		this.tbDownloadThread.Name = "tbDownloadThread";
		this.tbDownloadThread.Size = new System.Drawing.Size(55, 25);
		this.tbDownloadThread.TabIndex = 11;
		this.tbDownloadThread.Text = "4";
		this.tbDownloadThread.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tbDownloadThread.TextChanged += new System.EventHandler(tbDownloadThread_TextChanged);
		this.lblDownloadMaxTime.AutoSize = true;
		this.lblDownloadMaxTime.Location = new System.Drawing.Point(696, 86);
		this.lblDownloadMaxTime.Name = "lblDownloadMaxTime";
		this.lblDownloadMaxTime.Size = new System.Drawing.Size(113, 15);
		this.lblDownloadMaxTime.TabIndex = 12;
		this.lblDownloadMaxTime.Text = "最大时间(秒)：";
		this.tbDownloadMaxTime.Location = new System.Drawing.Point(810, 82);
		this.tbDownloadMaxTime.Name = "tbDownloadMaxTime";
		this.tbDownloadMaxTime.Size = new System.Drawing.Size(55, 25);
		this.tbDownloadMaxTime.TabIndex = 13;
		this.tbDownloadMaxTime.Text = "10";
		this.tbDownloadMaxTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tbDownloadMaxTime.TextChanged += new System.EventHandler(tbDownloadMaxTime_TextChanged);
		this.btnTestSpeed.Location = new System.Drawing.Point(885, 74);
		this.btnTestSpeed.Name = "btnTestSpeed";
		this.btnTestSpeed.Size = new System.Drawing.Size(130, 38);
		this.btnTestSpeed.TabIndex = 14;
		this.btnTestSpeed.Text = "测试下载";
		this.btnTestSpeed.UseVisualStyleBackColor = true;
		this.btnTestSpeed.Click += new System.EventHandler(btnTestSpeed_Click);
		// 右侧与底部操作栏
		this.cbKeywordFilter.AutoSize = true;
		this.cbKeywordFilter.Checked = true;
		this.cbKeywordFilter.CheckState = System.Windows.Forms.CheckState.Checked;
		this.cbKeywordFilter.Location = new System.Drawing.Point(1065, 30);
		this.cbKeywordFilter.Name = "cbKeywordFilter";
		this.cbKeywordFilter.Size = new System.Drawing.Size(112, 19);
		this.cbKeywordFilter.TabIndex = 20;
		this.cbKeywordFilter.Text = "关键词筛选:";
		this.toolTipKeywordFilter.SetToolTip(this.cbKeywordFilter, "勾选后启用关键词筛选规则");
		this.cbKeywordFilter.UseVisualStyleBackColor = true;
		this.cbKeywordFilter.CheckedChanged += new System.EventHandler(cbKeywordFilter_CheckedChanged);
		this.tbKeywordFilter.Location = new System.Drawing.Point(1185, 28);
		this.tbKeywordFilter.Name = "tbKeywordFilter";
		this.tbKeywordFilter.Size = new System.Drawing.Size(250, 25);
		this.tbKeywordFilter.TabIndex = 21;
		this.toolTipKeywordFilter.SetToolTip(this.tbKeywordFilter, "支持多个关键词，用逗号或空格分隔\r\n例如：香港,日本,美国 或 \ud83c\udded\ud83c\uddf0 \ud83c\uddef\ud83c\uddf5 \ud83c\uddfa\ud83c\uddf8\r\n使用 ! 前缀排除关键词\r\n例如：!过期 !失效");
		this.tbKeywordFilter.TextChanged += new System.EventHandler(tbKeywordFilter_TextChanged);
		this.btnKeywordPreset.Location = new System.Drawing.Point(1440, 28);
		this.btnKeywordPreset.Name = "btnKeywordPreset";
		this.btnKeywordPreset.Size = new System.Drawing.Size(33, 25);
		this.btnKeywordPreset.TabIndex = 22;
		this.btnKeywordPreset.Text = "▼";
		this.toolTipKeywordFilter.SetToolTip(this.btnKeywordPreset, "选择常用地区预设");
		this.btnKeywordPreset.UseVisualStyleBackColor = true;
		this.btnKeywordPreset.Click += new System.EventHandler(btnKeywordPreset_Click);
		this.btnClearLog.Location = new System.Drawing.Point(104, 142);
		this.btnClearLog.Name = "btnClearLog";
		this.btnClearLog.Size = new System.Drawing.Size(110, 38);
		this.btnClearLog.TabIndex = 25;
		this.btnClearLog.Text = "清空反馈";
		this.btnClearLog.UseVisualStyleBackColor = true;
		this.btnClearLog.Click += new System.EventHandler(btnClearLog_Click);
		this.btnClearResult.Location = new System.Drawing.Point(230, 142);
		this.btnClearResult.Name = "btnClearResult";
		this.btnClearResult.Size = new System.Drawing.Size(110, 38);
		this.btnClearResult.TabIndex = 26;
		this.btnClearResult.Text = "清空结果";
		this.btnClearResult.UseVisualStyleBackColor = true;
		this.btnClearResult.Click += new System.EventHandler(btnClearResult_Click);
		this.button1.ContextMenuStrip = this.contextMenuStrip1;
		this.button1.Location = new System.Drawing.Point(1450, 142);
		this.button1.Margin = new System.Windows.Forms.Padding(4);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(115, 38);
		this.button1.TabIndex = 27;
		this.button1.Text = "订阅管理";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click_1);
		this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
		this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.订阅列表ToolStripMenuItem, this.导入到节点列表ToolStripMenuItem });
		this.contextMenuStrip1.Name = "contextMenuStrip1";
		this.contextMenuStrip1.Size = new System.Drawing.Size(184, 52);
		this.订阅列表ToolStripMenuItem.Name = "订阅列表ToolStripMenuItem";
		this.订阅列表ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
		this.订阅列表ToolStripMenuItem.Text = "订阅列表";
		this.订阅列表ToolStripMenuItem.Click += new System.EventHandler(订阅列表ToolStripMenuItem_Click);
		this.导入到节点列表ToolStripMenuItem.Name = "导入到节点列表ToolStripMenuItem";
		this.导入到节点列表ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
		this.导入到节点列表ToolStripMenuItem.Text = "导入到节点列表";
		this.导入到节点列表ToolStripMenuItem.Click += new System.EventHandler(导入到节点列表ToolStripMenuItem_Click);
		this.button2.Location = new System.Drawing.Point(1580, 142);
		this.button2.Margin = new System.Windows.Forms.Padding(4);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(110, 38);
		this.button2.TabIndex = 28;
		this.button2.Text = "关于软件";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(button2_Click);
		this.cmsKeywordPreset.ImageScalingSize = new System.Drawing.Size(20, 20);
		this.cmsKeywordPreset.Name = "cmsKeywordPreset";
		this.cmsKeywordPreset.Size = new System.Drawing.Size(61, 4);
		this.cmsMain.ImageScalingSize = new System.Drawing.Size(20, 20);
		this.cmsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.menuExit });
		this.cmsMain.Name = "cmsMain";
		this.cmsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
		this.cmsMain.ShowCheckMargin = true;
		this.cmsMain.ShowImageMargin = false;
		this.cmsMain.Size = new System.Drawing.Size(109, 28);
		this.menuExit.Name = "menuExit";
		this.menuExit.Size = new System.Drawing.Size(108, 24);
		this.menuExit.Text = "退出";
		this.menuExit.Click += new System.EventHandler(menuExit_Click);
		this.AllowDrop = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1841, 1283);
		base.Controls.Add(this.splitContainer1);
		base.Margin = new System.Windows.Forms.Padding(5);
		this.MinimumSize = new System.Drawing.Size(1760, 1330);
		base.Name = "MainForm";
		this.Text = "nodesCatchNext - V4.0.0";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(MainForm_FormClosing);
		base.Load += new System.EventHandler(MainForm_Load);
		base.ResizeEnd += new System.EventHandler(MainForm_ResizeEnd);
		base.DragDrop += new System.Windows.Forms.DragEventHandler(MainForm_DragDrop);
		base.DragEnter += new System.Windows.Forms.DragEventHandler(MainForm_DragEnter);
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		this.cmsLv.ResumeLayout(false);
		this.panel2.ResumeLayout(false);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.panel1.ResumeLayout(false);
		this.groupBox3.ResumeLayout(false);
		this.groupBox5.ResumeLayout(false);
		this.groupBox5.PerformLayout();
		this.contextMenuStrip1.ResumeLayout(false);
		this.cmsMain.ResumeLayout(false);
		base.ResumeLayout(false);
	}

	private void InitializeSubscriptionTabControl()
	{
		tabSubscriptions = new TabControl();
		tabSubscriptions.Dock = DockStyle.None;
		tabSubscriptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		tabSubscriptions.Location = new Point(0, 0);
		tabSubscriptions.Name = "tabSubscriptions";
		tabSubscriptions.SelectedIndex = 0;
		tabSubscriptions.Size = new Size(splitContainer1.Panel1.Width, 26);
		tabSubscriptions.TabIndex = 0;
		tabSubscriptions.AllowDrop = true;
		tabSubscriptions.SelectedIndexChanged += tabSubscriptions_SelectedIndexChanged;
		tabSubscriptions.MouseDown += TabSubscriptions_MouseDown;
		tabSubscriptions.MouseMove += TabSubscriptions_MouseMove;
		tabSubscriptions.DragOver += TabSubscriptions_DragOver;
		tabSubscriptions.DragDrop += TabSubscriptions_DragDrop;
		cmsTabSubscriptions = new ContextMenuStrip();
		lvServers.Location = new Point(0, 26);
		lvServers.Height = splitContainer1.Panel1.Height - 26;
		lvServers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		lvServers.Dock = DockStyle.None;
		splitContainer1.Panel1.Controls.Add(tabSubscriptions);
		tabSubscriptions.BringToFront();
	}

	private void UpdateTabContextMenu()
	{
		cmsTabSubscriptions.Items.Clear();
		if (rightClickedTabSubId == null)
		{
			ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("更新所有订阅");
			toolStripMenuItem.Click += MenuUpdateSub_Click;
			ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem("删除所有节点");
			toolStripMenuItem2.Click += MenuDeleteAllNodes_Click;
			ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem("删除所有无分组节点");
			toolStripMenuItem3.Click += MenuDeleteUnassignedNodes_Click;
			cmsTabSubscriptions.Items.Add(toolStripMenuItem);
			cmsTabSubscriptions.Items.Add(toolStripMenuItem2);
			cmsTabSubscriptions.Items.Add(toolStripMenuItem3);
		}
		else if (rightClickedTabSubId == "__unassigned__")
		{
			ToolStripMenuItem toolStripMenuItem4 = new ToolStripMenuItem("删除所有无分组节点");
			toolStripMenuItem4.Click += MenuDeleteUnassignedNodes_Click;
			cmsTabSubscriptions.Items.Add(toolStripMenuItem4);
		}
		else
		{
			ToolStripMenuItem toolStripMenuItem5 = new ToolStripMenuItem("更新此订阅");
			toolStripMenuItem5.Click += MenuUpdateSub_Click;
			ToolStripMenuItem toolStripMenuItem6 = new ToolStripMenuItem("删除此订阅的所有节点");
			toolStripMenuItem6.Click += MenuDeleteSubNodes_Click;
			ToolStripMenuItem toolStripMenuItem7 = new ToolStripMenuItem("复制订阅地址");
			toolStripMenuItem7.Click += MenuCopySubUrl_Click;
			cmsTabSubscriptions.Items.Add(toolStripMenuItem5);
			cmsTabSubscriptions.Items.Add(toolStripMenuItem6);
			cmsTabSubscriptions.Items.Add(toolStripMenuItem7);
			cmsTabSubscriptions.Items.Add(new ToolStripSeparator());
			ToolStripMenuItem toolStripMenuItem8 = new ToolStripMenuItem("上移订阅");
			toolStripMenuItem8.Click += MenuMoveSubUp_Click;
			ToolStripMenuItem toolStripMenuItem9 = new ToolStripMenuItem("下移订阅");
			toolStripMenuItem9.Click += MenuMoveSubDown_Click;
			cmsTabSubscriptions.Items.Add(toolStripMenuItem8);
			cmsTabSubscriptions.Items.Add(toolStripMenuItem9);
		}
	}

	private void TabSubscriptions_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			for (int i = 0; i < tabSubscriptions.TabPages.Count; i++)
			{
				if (tabSubscriptions.GetTabRect(i).Contains(e.Location))
				{
					draggedTabIndex = i;
					Size dragSize = SystemInformation.DragSize;
					dragBoxFromMouseDown = new Rectangle(new Point(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2), dragSize);
					break;
				}
			}
		}
		else
		{
			if (e.Button != MouseButtons.Right)
			{
				return;
			}
			for (int j = 0; j < tabSubscriptions.TabPages.Count; j++)
			{
				if (tabSubscriptions.GetTabRect(j).Contains(e.Location))
				{
					TabPage tabPage = tabSubscriptions.TabPages[j];
					rightClickedTabSubId = tabPage.Tag as string;
					UpdateTabContextMenu();
					cmsTabSubscriptions.Show(tabSubscriptions, e.Location);
					break;
				}
			}
		}
	}

	private void TabSubscriptions_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && draggedTabIndex >= 0 && dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
		{
			if (draggedTabIndex > 0)
			{
				TabPage data = tabSubscriptions.TabPages[draggedTabIndex];
				tabSubscriptions.DoDragDrop(data, DragDropEffects.Move);
			}
			dragBoxFromMouseDown = Rectangle.Empty;
		}
	}

	private void TabSubscriptions_DragOver(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(typeof(TabPage)))
		{
			e.Effect = DragDropEffects.Move;
			Point pt = tabSubscriptions.PointToClient(new Point(e.X, e.Y));
			for (int i = 0; i < tabSubscriptions.TabPages.Count; i++)
			{
				if (tabSubscriptions.GetTabRect(i).Contains(pt))
				{
					if (i != draggedTabIndex && i > 0)
					{
						tabSubscriptions.Cursor = Cursors.Hand;
					}
					else
					{
						tabSubscriptions.Cursor = Cursors.No;
					}
					break;
				}
			}
		}
		else
		{
			e.Effect = DragDropEffects.None;
		}
	}

	private void TabSubscriptions_DragDrop(object sender, DragEventArgs e)
	{
		tabSubscriptions.Cursor = Cursors.Default;
		if (!e.Data.GetDataPresent(typeof(TabPage)))
		{
			return;
		}
		Point pt = tabSubscriptions.PointToClient(new Point(e.X, e.Y));
		int num = -1;
		for (int i = 0; i < tabSubscriptions.TabPages.Count; i++)
		{
			if (tabSubscriptions.GetTabRect(i).Contains(pt))
			{
				num = i;
				break;
			}
		}
		if (num < 0 || num == draggedTabIndex || draggedTabIndex < 1 || num < 1)
		{
			draggedTabIndex = -1;
			return;
		}
		string text = tabSubscriptions.TabPages[draggedTabIndex].Tag as string;
		if (string.IsNullOrEmpty(text) || config.subItem == null)
		{
			draggedTabIndex = -1;
			return;
		}
		int num2 = -1;
		int num3 = -1;
		for (int j = 0; j < config.subItem.Count; j++)
		{
			if (config.subItem[j].id == text)
			{
				num2 = j;
			}
			string text2 = tabSubscriptions.TabPages[num].Tag as string;
			if (!string.IsNullOrEmpty(text2) && config.subItem[j].id == text2)
			{
				num3 = j;
			}
		}
		if (num2 < 0 || num3 < 0)
		{
			draggedTabIndex = -1;
			return;
		}
		SubItem item = config.subItem[num2];
		config.subItem.RemoveAt(num2);
		if (num2 < num3)
		{
			config.subItem.Insert(num3 - 1, item);
		}
		else
		{
			config.subItem.Insert(num3, item);
		}
		ConfigHandler.SaveConfig(ref config, reload: false);
		InitSubscriptionTabs(restoreSelection: true);
		draggedTabIndex = -1;
		ShowMsg("订阅顺序已更新");
	}

	public void UpdateSingleSubscription(SubItem subItem, Action onFinished = null)
	{
		if (subItem == null || string.IsNullOrWhiteSpace(subItem.url))
		{
			ShowMsg("订阅信息无效或URL为空");
			onFinished?.Invoke();
			return;
		}
		string subRemarks = subItem.remarks;
		string url = subItem.url.Trim();
		string subId = subItem.id.Trim();
		AppendText(notify: false, "开始导入/更新订阅：" + subRemarks);
		AppendText(notify: false, "--> 正在下载订阅数据...");
		DownloadHandle downloadHandle = new DownloadHandle();
		downloadHandle.Error += delegate(object obj, ErrorEventArgs args)
		{
			AppendText(notify: false, "--> 下载失败：" + args.GetException().Message);
			ShowMsg("下载订阅失败：" + args.GetException().Message);
			onFinished?.Invoke();
		};
		downloadHandle.UpdateCompleted += delegate(object obj, DownloadHandle.ResultEventArgs args)
		{
			try
			{
				if (args.Success)
				{
					AppendText(notify: false, "--> 获取网页数据成功");
					string text = Utils.Base64Decode(args.Msg);
					int imported = 0;
					if (!Utils.IsNullOrEmpty(text))
					{
						if (text.IndexOf("vmess://") != -1 || text.IndexOf("vless://") != -1 || text.IndexOf("ss://") != -1 || text.IndexOf("ssr://") != -1 || text.IndexOf("trojan://") != -1 || text.IndexOf("socks://") != -1 || text.IndexOf("http://") != -1 || text.IndexOf("https://") != -1 || text.IndexOf("hysteria2://") != -1 || text.IndexOf("hy2://") != -1 || text.IndexOf("anytls://") != -1)
						{
							imported = MainFormHandler.Instance.AddBatchServers(config, text, subId);
						}
						else
						{
							AppendText(notify: false, "--> 尝试作为非Base64格式导入...");
							imported = MainFormHandler.Instance.AddBatchServers(config, args.Msg, subId);
						}
					}
					else
					{
						AppendText(notify: false, "--> Base64解码失败，尝试作为非Base64格式导入...");
						imported = MainFormHandler.Instance.AddBatchServers(config, args.Msg, subId);
					}

					if (imported <= 0)
					{
						AppendText(notify: false, "--> 导入节点信息失败");
						ShowMsg("导入节点信息失败");
					}
					else
					{
						AppendText(notify: false, "--> 节点信息更新完成");
						InitSubscriptionTabs(restoreSelection: true);
						RefreshServersView();
						ConfigHandler.ToJsonFile(config);
						ShowMsg($"订阅 \"{subRemarks}\" 导入/更新完成，共导入 {imported} 个节点");
					}
				}
				else
				{
					AppendText(notify: false, "--> 导入失败！" + args.Msg);
					ShowMsg("导入失败：" + args.Msg);
				}
			}
			finally
			{
				onFinished?.Invoke();
			}
		};
		downloadHandle.WebDownloadString(url);
	}

	private void MenuUpdateSub_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(rightClickedTabSubId))
		{
			UpdateSubscriptionProcess();
			return;
		}
		SubItem subItem = null;
		if (config.subItem != null)
		{
			foreach (SubItem item in config.subItem)
			{
				if (item.id == rightClickedTabSubId)
				{
					subItem = item;
					break;
				}
			}
		}
		UpdateSingleSubscription(subItem);
	}

	private void MenuDeleteSubNodes_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(rightClickedTabSubId))
		{
			return;
		}
		string text = "未知订阅";
		if (config.subItem != null)
		{
			foreach (SubItem item in config.subItem)
			{
				if (item.id == rightClickedTabSubId)
				{
					text = item.remarks;
					break;
				}
			}
		}
		if (MessageBox.Show("确定要删除订阅 \"" + text + "\" 的所有节点吗？\n\n此操作不可撤销！", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
		{
			return;
		}
		int num = 0;
		for (int num2 = config.vmess.Count - 1; num2 >= 0; num2--)
		{
			if (config.vmess[num2].subid == rightClickedTabSubId)
			{
				num++;
			}
		}
		for (int num3 = config.vmess.Count - 1; num3 >= 0; num3--)
		{
			if (config.vmess[num3].subid == rightClickedTabSubId)
			{
				config.vmess.RemoveAt(num3);
			}
		}
		ConfigHandler.SaveConfig(ref config, reload: false);
		RefreshServersView();
		ShowMsg($"已删除订阅 \"{text}\" 的 {num} 个节点");
	}

	private void MenuCopySubUrl_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(rightClickedTabSubId))
		{
			return;
		}
		if (config.subItem != null)
		{
			foreach (SubItem item in config.subItem)
			{
				if (item.id == rightClickedTabSubId)
				{
					if (!string.IsNullOrWhiteSpace(item.url))
					{
						Utils.SetClipboardData(item.url);
						ShowMsg("已复制订阅 \"" + item.remarks + "\" 的地址");
					}
					else
					{
						ShowMsg("该订阅地址为空");
					}
					return;
				}
			}
		}
		ShowMsg("未找到订阅信息");
	}

	private void MenuDeleteAllNodes_Click(object sender, EventArgs e)
	{
		if (config.vmess == null || config.vmess.Count == 0)
		{
			ShowMsg("当前没有可删除的节点");
		}
		else if (MessageBox.Show("确定要删除所有节点吗？\n\n此操作不可撤销！", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
		{
			int count = config.vmess.Count;
			config.vmess.Clear();
			ConfigHandler.SaveConfig(ref config, reload: false);
			RefreshServersView();
			ShowMsg($"已删除所有节点，共 {count} 个");
		}
	}

	private void MenuDeleteUnassignedNodes_Click(object sender, EventArgs e)
	{
		if (config.vmess == null || config.vmess.Count == 0)
		{
			ShowMsg("当前没有可删除的节点");
			return;
		}
		int num = config.vmess.RemoveAll((VmessItem v) => string.IsNullOrEmpty(v.subid));
		if (num == 0)
		{
			ShowMsg("没有找到未分组的节点");
			return;
		}
		ConfigHandler.SaveConfig(ref config, reload: false);
		InitSubscriptionTabs();
		RefreshServersView();
		ShowMsg($"已删除所有未分组的节点，共 {num} 个");
	}

	private void MenuMoveSubUp_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(rightClickedTabSubId) || config.subItem == null || config.subItem.Count <= 1)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < config.subItem.Count; i++)
		{
			if (config.subItem[i].id == rightClickedTabSubId)
			{
				num = i;
				break;
			}
		}
		if (num <= 0)
		{
			ShowMsg("此订阅已经在最上面，无法上移");
			return;
		}
		SubItem value = config.subItem[num];
		config.subItem[num] = config.subItem[num - 1];
		config.subItem[num - 1] = value;
		ConfigHandler.SaveConfig(ref config, reload: false);
		InitSubscriptionTabs(restoreSelection: true);
		ShowMsg("订阅已上移");
	}

	private void MenuMoveSubDown_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(rightClickedTabSubId) || config.subItem == null || config.subItem.Count <= 1)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < config.subItem.Count; i++)
		{
			if (config.subItem[i].id == rightClickedTabSubId)
			{
				num = i;
				break;
			}
		}
		if (num < 0 || num >= config.subItem.Count - 1)
		{
			ShowMsg("此订阅已经在最下面，无法下移");
			return;
		}
		SubItem value = config.subItem[num];
		config.subItem[num] = config.subItem[num + 1];
		config.subItem[num + 1] = value;
		ConfigHandler.SaveConfig(ref config, reload: false);
		InitSubscriptionTabs(restoreSelection: true);
		ShowMsg("订阅已下移");
	}
}
