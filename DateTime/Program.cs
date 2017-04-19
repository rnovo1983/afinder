using System;
using Amazon.EC2;
using Amazon.EC2.Model;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using MonoDevelop.MacInterop;






namespace ec2search
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			string profile = string.Empty, filterx = string.Empty, key=string.Empty, sec = string.Empty;
			AmazonEC2Client client = null;
			List<chost> hosts = new List<chost>();
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.ForegroundColor = ConsoleColor.White;

			if (args.Length > 0)
			{
				profile = args[0];
				if (args.Length > 1)
					filterx = args[1];
				else
					filterx = "*";
			}


			if (GetProfile(profile) != null)
			{
				 key = GetProfile(profile)[0];
				 sec = GetProfile(profile)[1];
			}
			else
			{
				Console.WriteLine("Invalid, or no existent AWS Profile -->"+profile);
				Environment.Exit(1);
			}

			try
			{
			       client = new AmazonEC2Client(key, sec);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Invalid Key/Secret "+ex.Message);
				Environment.Exit(1);
			}


			var request = new DescribeInstancesRequest()
			{
				Filter = new List<Filter>()
			{
			       new Filter()
			       {
					Name = "tag-value",
						Value = new List<String>
					{
					      "*"
					}
				   }
		       }
			};
			var response = client.DescribeInstances(request);
			int c = 0;
			string id = string.Empty, name = string.Empty, type = string.Empty, state = string.Empty, privip = string.Empty, keyname = string.Empty;
			List<string[]> resultsX = new List<string[]>();

			resultsX.Add(new string[] { "Count", "ID", "Private IP", "State", "Type", "Name", "Key" });
			foreach (var ec2instace in response.DescribeInstancesResult.Reservation)
			{
				try
				{
					if (filterx != "*")
					{
						if (ec2instace.RunningInstance[0].Tag.Find((obj) => obj.Key.Contains("Name")).Value.Contains(filterx))
						{
							c++;
							id = ec2instace.RunningInstance[0].InstanceId;
							name = ec2instace.RunningInstance[0].Tag.Find((obj) => obj.Key.Contains("Name")).Value;
							type = ec2instace.RunningInstance[0].InstanceType;
							state = ec2instace.RunningInstance[0].InstanceState.Name;
							privip = ec2instace.RunningInstance[0].PrivateIpAddress;
							keyname = ec2instace.RunningInstance[0].KeyName;
							resultsX.Add(new string[] { c.ToString(), id, privip, state, type, name, keyname });
						}
					}
					else
					{
						c++;
						id = ec2instace.RunningInstance[0].InstanceId;
						name = ec2instace.RunningInstance[0].Tag.Find((obj) => obj.Key.Contains("Name")).Value;
						type = ec2instace.RunningInstance[0].InstanceType;
						state = ec2instace.RunningInstance[0].InstanceState.Name;
						privip = ec2instace.RunningInstance[0].PrivateIpAddress;
						keyname = ec2instace.RunningInstance[0].KeyName;
						resultsX.Add(new string[] { c.ToString(), id, privip, state, type, name, keyname });
					}

				}
				catch
				{
					Console.WriteLine("filter not valid");
				}
			}
			#region Main Body

			Console.WriteLine(ArrayPrinter.GetDataInTableFormat(resultsX));
			ArrayList hosts_list = AddHosts(resultsX);
			ArrayList final = new ArrayList();
			string selection;

			Console.WriteLine("Enter Instance/s Number To Access it :");
			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write("#-> ");
			selection = Console.ReadLine();
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
			final.AddRange(selection.Split(','));
			string iph, pemh, nameh;
			for (int i = 0; i < final.Count; i++)
			{
				iph = GETHOST(final[i].ToString(), hosts_list).ip;
				pemh = GETHOST(final[i].ToString(), hosts_list).pem;
				nameh = GETHOST(final[i].ToString(), hosts_list).hostname;

				 if (i == 0)
				{
					hsplit();
					ssh(pemh,iph,nameh);

					if (final.Count > 1){vsplit();}  //DONE
				}
			         if (i == 1)
				{
					ssh(pemh, iph,nameh);
					if (final.Count > 2){MoveR(); CloseSession();}  //DONE
				}
			         if(i == 2)
				{
					hsplit();
					ssh(pemh, iph,nameh);

					if (final.Count > 3) { MoveR();} //DONE

				}
				 if (i == 3)
				{
					hsplit();
					ssh(pemh, iph,nameh);
					if (final.Count > 4) { hsplit(); } //DONE
				}
				 if (i == 4)
				{
					MoveR();
					ssh(pemh, iph,nameh); //DONE
					if (final.Count > 5) { MoveR(); hsplit(); } //DONE
				}
			         if (i == 5)
				{
					ssh(pemh, iph,nameh);
				}
			}

			#endregion
		}
		public static string [] GetProfile(string profile)
		{
			string[] result = new string[2];
			string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.aws/credentials";
			string [] lines = File.ReadAllLines(path);
			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].Contains(profile))
				{
					result[0] = lines[i + 1].Substring(20,20);
					result[1] = lines[i + 2].Substring(24,40);
				}
			}
			return result;
		}
		public static  ArrayList AddHosts(List<string[]> result)
		{
			ArrayList aux = new ArrayList(result.Count-1);
			string ip, hostname, keyfile, type, count; 

			for (int i = 1; i < result.Count; i++)
			{
				ip = result[i][2];
				hostname = result[i][5];
				keyfile = result[i][6];
				type = result[i][4];
				count = result[i][0];

				aux.Add(new chost(ip, hostname, keyfile, type, count));
			}
			return aux;
		}
		public static void ssh (string pem, string ip, string name)
		{
			string script = @"
			tell application ""iTerm2""
                           activate
			     tell current session of current window
                                write text ""clear""
                                write text ""echo 'Session to *****{3}*****'""
                                write text ""ssh -i /Users/{0}/.ssh/{1}.pem ubuntu@{2}""
                             end tell
                        end tell";

			MonoDevelop.MacInterop.AppleScript.Run(string.Format(script, Environment.UserName, pem, ip,name));

		}
		public static void hsplit()
		{
			string script = @"tell application ""System Events"" to keystroke ""d"" using {command down, shift down}";
			MonoDevelop.MacInterop.AppleScript.Run(script);

		}
		public static void vsplit()
		{
			string script = @"tell application ""System Events"" to keystroke ""d"" using command down";
			MonoDevelop.MacInterop.AppleScript.Run(script);
		}
		public static void MoveR()
		{
			string script = @"tell application ""System Events"" to keystroke ""]"" using command down";
			MonoDevelop.MacInterop.AppleScript.Run(script);

		}
		public static void CloseSession()
		{
			string script = @"
			tell application ""iTerm2""
                           activate
			     tell current session of current window
                               close
                             end tell
                        end tell";

			MonoDevelop.MacInterop.AppleScript.Run(script);

		}
		public static void MoveL()
		{
			string script = @"tell application ""System Events"" to keystroke ""["" using command down";

			MonoDevelop.MacInterop.AppleScript.Run(script);

		}
		public static chost GETHOST(string count, ArrayList hosts)
		{
				for (int i = 0; i < hosts.Count; i++)
				{
				     if ((hosts[i] as chost).count == count)
					{
					return (hosts[i] as chost);
					}
				}

			return null;
		}

	}
}
