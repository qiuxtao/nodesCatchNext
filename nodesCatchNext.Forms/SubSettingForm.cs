using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using nodesCatchNext.Handler;
using nodesCatchNext.Mode;

namespace nodesCatchNext.Forms;

public class SubSettingForm : Form
{
	private List<SubSettingControl> lstControls = new List<SubSettingControl>();

	private IContainer components;

	private Panel panel1;

	private Button btnCancel;

	private Button btnAdd;

	private Button btnEnableAll;

	private Button btnDisableAll;

	private Panel panCon;

	public SubSettingForm()
	{
		InitializeComponent();
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
	}

	private void SubSettingForm_Load(object sender, EventArgs e)
	{
		if (MainForm.config == null)
		{
			MainForm.config = new Config();
		}
		if (MainForm.config.subItem == null)
		{
			MainForm.config.subItem = new List<SubItem>();
		}
		RefreshSubsView();
	}

	private void RefreshSubsView()
	{
		panCon.Controls.Clear();
		panCon.Visible = false;
		lstControls.Clear();
		for (int num = MainForm.config.subItem.Count - 1; num >= 0; num--)
		{
			SubItem subItem = MainForm.config.subItem[num];
			if (Utils.IsNullOrEmpty(subItem.remarks) && Utils.IsNullOrEmpty(subItem.url))
			{
				MainForm.config.subItem.RemoveAt(num);
			}
		}
		foreach (SubItem item in MainForm.config.subItem)
		{
			SubSettingControl subSettingControl = new SubSettingControl();
			subSettingControl.OnButtonClicked += Control_OnButtonClicked;
			subSettingControl.subItem = item;
			subSettingControl.Dock = DockStyle.Top;
			panCon.Controls.Add(subSettingControl);
			lstControls.Add(subSettingControl);
		}
		panCon.Visible = true;
		MainForm.Instance?.InitSubscriptionTabs();
	}

	private void Control_OnButtonClicked(object sender, EventArgs e)
	{
		RefreshSubsView();
	}


	private void SubSettingForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		for (int num = MainForm.config.subItem.Count - 1; num >= 0; num--)
		{
			SubItem subItem = MainForm.config.subItem[num];
			if (Utils.IsNullOrEmpty(subItem.remarks) && Utils.IsNullOrEmpty(subItem.url))
			{
				MainForm.config.subItem.RemoveAt(num);
			}
		}
		ConfigHandler.SaveSubItem(ref MainForm.config);
		MainForm.Instance?.InitSubscriptionTabs();
		MainForm.Instance?.RefreshServersView();
		ConfigHandler.ToJsonFile(MainForm.config);
	}

	private void AddSub()
	{
		SubItem item = new SubItem
		{
			id = Utils.GetGUID(),
			remarks = "分组",
			url = string.Empty
		};
		MainForm.config.subItem.Add(item);
	}

	private void btnAdd_Click(object sender, EventArgs e)
	{
		AddSub();
		RefreshSubsView();
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
	}

	private void btnEnableAll_Click(object sender, EventArgs e)
	{
		foreach (SubItem item in MainForm.config.subItem)
		{
			item.enabled = true;
		}
		RefreshSubsView();
	}

	private void btnDisableAll_Click(object sender, EventArgs e)
	{
		foreach (SubItem item in MainForm.config.subItem)
		{
			item.enabled = false;
		}
		RefreshSubsView();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.panel1 = new System.Windows.Forms.Panel();
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnAdd = new System.Windows.Forms.Button();
		this.btnEnableAll = new System.Windows.Forms.Button();
		this.btnDisableAll = new System.Windows.Forms.Button();
		this.panCon = new System.Windows.Forms.Panel();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.panel1.Controls.Add(this.btnCancel);
		this.panel1.Controls.Add(this.btnAdd);
		this.panel1.Controls.Add(this.btnEnableAll);
		this.panel1.Controls.Add(this.btnDisableAll);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panel1.Location = new System.Drawing.Point(0, 464);
		this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(632, 60);
		this.panel1.TabIndex = 0;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(263, 18);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(104, 30);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Visible = false;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnAdd.Location = new System.Drawing.Point(15, 13);
		this.btnAdd.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.btnAdd.Name = "btnAdd";
		this.btnAdd.Size = new System.Drawing.Size(145, 34);
		this.btnAdd.TabIndex = 0;
		this.btnAdd.Text = "新增分组/订阅";
		this.btnAdd.UseVisualStyleBackColor = true;
		this.btnAdd.Click += new System.EventHandler(btnAdd_Click);
		this.btnEnableAll.Location = new System.Drawing.Point(170, 13);
		this.btnEnableAll.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.btnEnableAll.Name = "btnEnableAll";
		this.btnEnableAll.Size = new System.Drawing.Size(135, 34);
		this.btnEnableAll.TabIndex = 3;
		this.btnEnableAll.Text = "一键启用全部";
		this.btnEnableAll.UseVisualStyleBackColor = true;
		this.btnEnableAll.Click += new System.EventHandler(btnEnableAll_Click);
		this.btnDisableAll.Location = new System.Drawing.Point(315, 13);
		this.btnDisableAll.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.btnDisableAll.Name = "btnDisableAll";
		this.btnDisableAll.Size = new System.Drawing.Size(135, 34);
		this.btnDisableAll.TabIndex = 4;
		this.btnDisableAll.Text = "一键禁用全部";
		this.btnDisableAll.UseVisualStyleBackColor = true;
		this.btnDisableAll.Click += new System.EventHandler(btnDisableAll_Click);
		this.panCon.AutoScroll = true;
		this.panCon.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panCon.Location = new System.Drawing.Point(0, 0);
		this.panCon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.panCon.Name = "panCon";
		this.panCon.Size = new System.Drawing.Size(632, 464);
		this.panCon.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(720, 524);
		base.Controls.Add(this.panCon);
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
		base.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		base.MaximizeBox = true;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(650, 420);
		base.Name = "SubSettingForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "分组/订阅列表管理";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(SubSettingForm_FormClosing);
		base.Load += new System.EventHandler(SubSettingForm_Load);
		this.panel1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
