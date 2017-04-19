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

	}
}
