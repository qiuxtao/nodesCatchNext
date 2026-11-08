using System;
using System.Collections.Generic;
using nodesCatchNext.Base;

namespace nodesCatchNext.Mode;

[Serializable]
public class Config
{
	public int index { get; set; }

	public int localPort { get; set; }

	public int externalControllerPort { get; set; }

	public string externalController { get; set; }

	public ECoreType coreType { get; set; } = ECoreType.mihomo_core;

	public bool defAllowInsecure { get; set; } = true;

	public string speedPingTestUrl { get; set; }

	public string PingCount { get; set; }

	public string Timeout { get; set; }

	public string speedTestUrl { get; set; }

	public string DownloadThread { get; set; }

	public string DownloadMaxTime { get; set; }

	public List<VmessItem> vmess { get; set; }

	public List<SubItem> subItem { get; set; }

	public UIItem uiItem { get; set; }

	public string keywordFilter { get; set; }

	public bool keywordFilterEnabled { get; set; } = true;

	public int GetLocalPort(string protocol = "w")
	{
		return localPort + 10001;
	}

	public string address()
	{
		if (index < 0)
		{
			return string.Empty;
		}
		return vmess[index].address.TrimEx();
	}

	public int port()
	{
		if (index < 0)
		{
			return 10808;
		}
		return vmess[index].port;
	}

	public string id()
	{
		if (index < 0)
		{
			return string.Empty;
		}
		return vmess[index].id.TrimEx();
	}

	public int alterId()
	{
		if (index < 0)
		{
			return 0;
		}
		return vmess[index].alterId;
	}

	public string security()
	{
		if (index < 0)
		{
			return string.Empty;
		}
		return vmess[index].security.TrimEx();
	}

	public string remarks()
	{
		if (index < 0)
		{
			return string.Empty;
		}
		return vmess[index].remarks.TrimEx();
	}

	public string network()
	{
		if (index < 0 || Utils.IsNullOrEmpty(vmess[index].network))
		{
			return "tcp";
		}
		return vmess[index].network.TrimEx();
	}

	public string headerType()
	{
		if (index < 0 || Utils.IsNullOrEmpty(vmess[index].headerType))
		{
			return "none";
		}
		return vmess[index].headerType.Replace(" ", "").TrimEx();
	}

	public string requestHost()
	{
		if (index < 0 || Utils.IsNullOrEmpty(vmess[index].requestHost))
		{
			return string.Empty;
		}
		return vmess[index].requestHost.Replace(" ", "").TrimEx();
	}

	public string path()
	{
		if (index < 0 || Utils.IsNullOrEmpty(vmess[index].path))
		{
			return string.Empty;
		}
		return vmess[index].path.Replace(" ", "").TrimEx();
	}

	public string streamSecurity()
	{
		if (index < 0 || Utils.IsNullOrEmpty(vmess[index].streamSecurity))
		{
			return string.Empty;
		}
		return vmess[index].streamSecurity;
	}

	public bool allowInsecure()
	{
		if (index < 0 || Utils.IsNullOrEmpty(vmess[index].allowInsecure))
		{
			return defAllowInsecure;
		}
		return Convert.ToBoolean(vmess[index].allowInsecure);
	}

	public int configType()
	{
		if (index < 0)
		{
			return 0;
		}
		return vmess[index].configType;
	}

	public string getItemId()
	{
		if (index < 0)
		{
			return string.Empty;
		}
		return vmess[index].getItemId();
	}

	public string flow()
	{
		if (index < 0)
		{
			return string.Empty;
		}
		return vmess[index].flow.TrimEx();
	}

	public string sni()
	{
		if (index < 0)
		{
			return string.Empty;
		}
		return vmess[index].sni.TrimEx();
	}

	public string publicKey()
	{
		if (index < 0)
		{
			return string.Empty;
		}
		return vmess[index].publicKey?.TrimEx() ?? string.Empty;
	}

	public string shortId()
	{
		if (index < 0)
		{
			return string.Empty;
		}
		return vmess[index].shortId?.TrimEx() ?? string.Empty;
	}
}
