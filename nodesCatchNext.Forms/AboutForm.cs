using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace nodesCatchNext.Forms;

public class AboutForm : Form
{
	private IContainer components;

	private Label labelProject;

	private LinkLabel linkLabelProject;

	private Label label2;

	private LinkLabel linkLabel2;

	private GroupBox groupBox1;

	private LinkLabel linkLabel5;

	private Label label7;

	private Label label6;

	private GroupBox groupBox2;

	private Label label3;

	private Label labelNodesCatch;

	private LinkLabel linkLabelNodesCatch;

	public AboutForm()
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

	private void linkLabelProject_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo(linkLabelProject.Text) { UseShellExecute = true });
		}
		catch
		{
		}
	}

	private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo(linkLabel2.Text) { UseShellExecute = true });
		}
		catch
		{
		}
	}

	private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo(linkLabel5.Text) { UseShellExecute = true });
		}
		catch
		{
		}
	}

	private void linkLabelNodesCatch_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo(linkLabelNodesCatch.Text) { UseShellExecute = true });
		}
		catch
		{
		}
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
		this.labelProject = new System.Windows.Forms.Label();
		this.linkLabelProject = new System.Windows.Forms.LinkLabel();
		this.label2 = new System.Windows.Forms.Label();
		this.linkLabel2 = new System.Windows.Forms.LinkLabel();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.label6 = new System.Windows.Forms.Label();
		this.linkLabel5 = new System.Windows.Forms.LinkLabel();
		this.label7 = new System.Windows.Forms.Label();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.labelNodesCatch = new System.Windows.Forms.Label();
		this.linkLabelNodesCatch = new System.Windows.Forms.LinkLabel();
		this.label3 = new System.Windows.Forms.Label();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		base.SuspendLayout();
		// 
		// labelProject
		// 
		this.labelProject.AutoSize = true;
		this.labelProject.Font = new System.Drawing.Font("微软雅黑", 13f);
		this.labelProject.Location = new System.Drawing.Point(20, 30);
		this.labelProject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelProject.Name = "labelProject";
		this.labelProject.Size = new System.Drawing.Size(123, 30);
		this.labelProject.TabIndex = 0;
		this.labelProject.Text = "项目地址：";
		// 
		// linkLabelProject
		// 
		this.linkLabelProject.AutoSize = true;
		this.linkLabelProject.Font = new System.Drawing.Font("Consolas", 12.5f);
		this.linkLabelProject.Location = new System.Drawing.Point(100, 60);
		this.linkLabelProject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.linkLabelProject.Name = "linkLabelProject";
		this.linkLabelProject.Size = new System.Drawing.Size(478, 24);
		this.linkLabelProject.TabIndex = 1;
		this.linkLabelProject.TabStop = true;
		this.linkLabelProject.Text = "https://github.com/qiuxtao/nodesCatchNext";
		this.linkLabelProject.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabelProject_LinkClicked);
		// 
		// label2
		// 
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("微软雅黑", 13f);
		this.label2.Location = new System.Drawing.Point(20, 95);
		this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(123, 30);
		this.label2.TabIndex = 2;
		this.label2.Text = "Mihomo：";
		// 
		// linkLabel2
		// 
		this.linkLabel2.AutoSize = true;
		this.linkLabel2.Font = new System.Drawing.Font("Consolas", 12.5f);
		this.linkLabel2.Location = new System.Drawing.Point(100, 125);
		this.linkLabel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.linkLabel2.Name = "linkLabel2";
		this.linkLabel2.Size = new System.Drawing.Size(430, 24);
		this.linkLabel2.TabIndex = 3;
		this.linkLabel2.TabStop = true;
		this.linkLabel2.Text = "https://github.com/MetaCubeX/mihomo";
		this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel2_LinkClicked);
		// 
		// label7
		// 
		this.label7.AutoSize = true;
		this.label7.Font = new System.Drawing.Font("微软雅黑", 13f);
		this.label7.Location = new System.Drawing.Point(20, 160);
		this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(176, 30);
		this.label7.TabIndex = 4;
		this.label7.Text = "Subconverter：";
		// 
		// linkLabel5
		// 
		this.linkLabel5.AutoSize = true;
		this.linkLabel5.Font = new System.Drawing.Font("Consolas", 12.5f);
		this.linkLabel5.Location = new System.Drawing.Point(100, 190);
		this.linkLabel5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.linkLabel5.Name = "linkLabel5";
		this.linkLabel5.Size = new System.Drawing.Size(502, 24);
		this.linkLabel5.TabIndex = 5;
		this.linkLabel5.TabStop = true;
		this.linkLabel5.Text = "https://github.com/asdlokj1qpi233/subconverter";
		this.linkLabel5.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel5_LinkClicked);
		// 
		// labelNodesCatch
		// 
		this.labelNodesCatch.AutoSize = true;
		this.labelNodesCatch.Font = new System.Drawing.Font("微软雅黑", 13f);
		this.labelNodesCatch.Location = new System.Drawing.Point(20, 225);
		this.labelNodesCatch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelNodesCatch.Name = "labelNodesCatch";
		this.labelNodesCatch.Size = new System.Drawing.Size(211, 30);
		this.labelNodesCatch.TabIndex = 6;
		this.labelNodesCatch.Text = "nodesCatch V2.0：";
		// 
		// linkLabelNodesCatch
		// 
		this.linkLabelNodesCatch.AutoSize = true;
		this.linkLabelNodesCatch.Font = new System.Drawing.Font("Consolas", 12.5f);
		this.linkLabelNodesCatch.Location = new System.Drawing.Point(100, 255);
		this.linkLabelNodesCatch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.linkLabelNodesCatch.Name = "linkLabelNodesCatch";
		this.linkLabelNodesCatch.Size = new System.Drawing.Size(538, 24);
		this.linkLabelNodesCatch.TabIndex = 7;
		this.linkLabelNodesCatch.TabStop = true;
		this.linkLabelNodesCatch.Text = "https://bulianglin.com/archives/nodescatch.html";
		this.linkLabelNodesCatch.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabelNodesCatch_LinkClicked);
		// 
		// label3
		// 
		this.label3.AutoSize = true;
		this.label3.Font = new System.Drawing.Font("微软雅黑", 11f);
		this.label3.Location = new System.Drawing.Point(20, 295);
		this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(653, 50);
		this.label3.TabIndex = 8;
		this.label3.Text = "支持测速协议：Shadowsocks、ShadowsocksR、VMess、VLESS\r\n                        Trojan、Socks5、HTTP(S)、Hysteria2、AnyTLS";
		// 
		// groupBox2
		// 
		this.groupBox2.Controls.Add(this.labelProject);
		this.groupBox2.Controls.Add(this.linkLabelProject);
		this.groupBox2.Controls.Add(this.label2);
		this.groupBox2.Controls.Add(this.linkLabel2);
		this.groupBox2.Controls.Add(this.label7);
		this.groupBox2.Controls.Add(this.linkLabel5);
		this.groupBox2.Controls.Add(this.labelNodesCatch);
		this.groupBox2.Controls.Add(this.linkLabelNodesCatch);
		this.groupBox2.Controls.Add(this.label3);
		this.groupBox2.Font = new System.Drawing.Font("微软雅黑", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.groupBox2.Location = new System.Drawing.Point(9, 15);
		this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
		this.groupBox2.Size = new System.Drawing.Size(764, 360);
		this.groupBox2.TabIndex = 0;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "基于与引用";
		// 
		// groupBox1
		// 
		this.groupBox1.Controls.Add(this.label6);
		this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.groupBox1.Location = new System.Drawing.Point(9, 390);
		this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
		this.groupBox1.Size = new System.Drawing.Size(764, 150);
		this.groupBox1.TabIndex = 1;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "免责声明";
		// 
		// label6
		// 
		this.label6.Font = new System.Drawing.Font("微软雅黑", 12.5f);
		this.label6.Location = new System.Drawing.Point(25, 35);
		this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(710, 96);
		this.label6.TabIndex = 0;
		this.label6.Text = "        本软件仅供非中国大陆地区用户学习交流使用，禁止在中国大陆传播使用，请务必遵守所在国法律法规，任何使用后果与软件作者无关！";
		// 
		// AboutForm
		// 
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(784, 555);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.groupBox1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.KeyPreview = true;
		base.KeyDown += (s, e) => { if (e.KeyCode == System.Windows.Forms.Keys.Escape) this.Close(); };
		base.Margin = new System.Windows.Forms.Padding(4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowIcon = true;
		this.MaximumSize = new System.Drawing.Size(802, 605);
		this.MinimumSize = new System.Drawing.Size(802, 605);
		base.Name = "AboutForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "关于";
		this.groupBox1.ResumeLayout(false);
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		base.ResumeLayout(false);
	}
}
