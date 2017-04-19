using System;
using System.Diagnostics;
using MonoDevelop.MacInterop;

namespace ec2search
{
	public class chost
	{
		public string ip;
		public string hostname;
		public string pem;
		public string ec2type;
		public string count;


		public chost(string ip, string hostname, string pem, string ec2type, string count)
		{
			this.ip = ip;
			this.hostname = hostname;
			this.pem = pem;
			this.ec2type = ec2type;
			this.count = count;
		}
		public bool ssh()
		{
			try
			{
				//string script = string.Format("tell application \"System Events\" to keystroke \"ssh -i /Users/{0}/.ssh/{1}.pem ubuntu@{2}\"",Environment.UserName, pem, ip);
				string script = string.Format("tell application \"System Events\" to keystroke \"ssh -i /Users/rnovo/.ssh/cc_shared.pem ubuntu@10.0.16.166\"");
				MonoDevelop.MacInterop.AppleScript.Run(script);
			}
			catch { return false;  }
			return true;
		}
	}
}
