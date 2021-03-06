﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;
using System.Configuration;
using System.Diagnostics;

namespace CodeBrowserDeluxePro
{
	public partial class Form1 : Form
	{
		private ScintillaNET.Scintilla TextArea;
		private HttpClient client;
		private ScintillaHelper sh;
		private string workingDirectory;
		private string startPath;
		public Form1(string theStartPath)
		{
			startPath = theStartPath;
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{

			string workspace = ConfigurationManager.AppSettings["workspace"];
			workingDirectory = ConfigurationManager.AppSettings["workingDirectory"];
			TextArea = new ScintillaNET.Scintilla();
			splitContainer1.Panel2.Controls.Add(TextArea);

			// BASIC CONFIG
			TextArea.Dock = System.Windows.Forms.DockStyle.Fill;
			TextArea.TextChanged += (this.OnTextChanged);
			tvFiles.MouseDown += (this.onNodeMouseClick);

			client = new HttpClient();
			//tvFiles.Nodes.AddRange()
			//string workspace = @"C:\Users\kevin\code";
			tvFiles.Nodes.AddRange(GetNodes(workspace));
			
			sh = new ScintillaHelper { TextArea = TextArea };

			sh.Init(Syntax.XML);

			if (startPath !=null && startPath.Length > 0)
			{
				string[] arrStartPath = startPath.Replace(workspace, "").Split('\\');
				foreach(string pathPart in arrStartPath)
				{
					if(pathPart.Length> 0)
					{
						NodeNavigate(pathPart);
					}
				}
			}
		}

		private async void NodeNavigate(string pathPart)
		{
			var nodes = tvFiles.Nodes.Find(pathPart, false);
			if(nodes.Length > 0)
			{
				var node = nodes[0];
				tvFiles.SelectedNode = node;
				var x = new TreeViewEventArgs(node);
				//this is async and you need to await it
				tvFiles_AfterSelect(tvFiles, x);
				node.Expand();
			}
			
		}

		private void onNodeMouseClick(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Middle)
			{
				var node = (MyTreeNode)tvFiles.SelectedNode;
				ProcessStartInfo p1 = new ProcessStartInfo("CodeBrowserDeluxePro.exe");
				p1.WorkingDirectory = workingDirectory;
				p1.Arguments = String.Format("--path \"{0}\"", node.ThePath);
				Process process = Process.Start(p1);
			}
		}



		private static bool IsJSFile(string f)
		{
			return f != null &&
				f.EndsWith(".js", StringComparison.Ordinal);
		}


		private MyTreeNode[] GetNodes(string path)
		{
			//string[] entries = Directory.GetFileSystemEntries(@"C:\Users\kevin\code", "*", SearchOption.AllDirectories);
			var files = Directory.GetFiles(path).Select(f => new MyTreeNode
			{
				Text = Path.GetFileName(f),
				Name = Path.GetFileName(f),
				NodeType = IsJSFile(f) ? NodeType.JSFile : NodeType.File,
				ThePath = f
			});

			var folders = Directory.GetDirectories(path).Select(d => new MyTreeNode
			{
				Text = Path.GetFileName(d),
				Name = Path.GetFileName(d),
				NodeType = NodeType.Folder,
				ThePath = d
			});

			List<MyTreeNode> list = new List<MyTreeNode>();
			list.AddRange(files);
			list.AddRange(folders);
			return list.ToArray();
		}
		
		private async Task<string> requestTopLevel(string ThePath)
		{
			var values = new Dictionary<string, string>
			{
			   { "fileName", ThePath }
			};

			var content = new FormUrlEncodedContent(values);

			var response = await client.PostAsync("http://localhost:3000/parse", content);

			return await response.Content.ReadAsStringAsync();
		}

		private void OnTextChanged(object sender, EventArgs e)
		{

		}

		private Syntax ExtensionToSyntax(string path)
		{
			StringComparison sc = StringComparison.OrdinalIgnoreCase;
			if (path.EndsWith(".cpp", sc))
			{
				return Syntax.CPP;
			}
			else if (path.EndsWith(".css", sc))
			{
				return Syntax.CSS;
			}
			else if (path.EndsWith(".html", sc) || path.EndsWith(".htm", sc))
			{
				return Syntax.HTML;
			}
			else if (path.EndsWith(".js", sc))
			{
				return Syntax.JAVASCRIPT;
			}
			else if (path.EndsWith(".json", sc))
			{
				return Syntax.JSON;
			}
			else if (path.EndsWith(".jsx", sc))
			{
				return Syntax.JSX;
			}
			else if (path.EndsWith(".less", sc))
			{
				return Syntax.LESS;
			}
			if (path.EndsWith(".scss", sc))
			{
				return Syntax.SASS;
			}
			if (path.EndsWith(".ts", sc))
			{
				return Syntax.TS;
			}
			if (path.EndsWith(".txt", sc))
			{
				return Syntax.TXT;
			}
			if (path.EndsWith(".xml", sc))
			{
				return Syntax.XML;
			}
			else
			{
				return Syntax.TXT;
			}
		}

		private async void tvFiles_AfterSelect(object sender, TreeViewEventArgs e)
		{
			var node = (MyTreeNode)e.Node;
			var isLoaded = node.isLoaded;
			switch (node.NodeType)
			{
				case NodeType.Folder:
					{
						if (!isLoaded)
						{
							tvFiles.SelectedNode.Nodes.AddRange(GetNodes(node.ThePath));
							node.isLoaded = true;
						}
						TextArea.Text = "";
						break;
					}
				case NodeType.File:
					{
						TextArea.Text = File.ReadAllText(node.ThePath);
						sh.Init(ExtensionToSyntax(node.ThePath));
						break;
					}
				case NodeType.JSFile:
					{
						sh.Init(Syntax.JAVASCRIPT);
						if (!isLoaded)
						{
							try
							{
								string resp = await requestTopLevel(node.ThePath);
								tvFiles.SelectedNode.Nodes.AddRange(GetCodeNodes(resp));
								node.isLoaded = true;
							}
							catch (Exception ex)
							{
								MessageBox.Show("Please start the server");
							}
						}
						TextArea.Text = File.ReadAllText(node.ThePath);
						break;
					}
				case NodeType.Code:
					{
						sh.Init(Syntax.JAVASCRIPT);
						string fileText = File.ReadAllText(((MyTreeNode)node.Parent).ThePath);
						string chunk = fileText.Substring(node.Start, node.End - node.Start);
						TextArea.Text = chunk;
						break;
					}
				default:
					{
						break;
					}
			}
		}

		private MyTreeNode[] GetCodeNodes(string json)
		{
			return JsonConvert.DeserializeObject<List<Code>>(json).Select(n => new MyTreeNode
			{
				Text = n.name,
				NodeType = NodeType.Code,
				Start = n.start,
				End = n.end
			}).ToArray();

		}
	}

	public class Code
	{
		public string name { get; set; }
		public int start { get; set; }
		public int end { get; set; }
	}

	public enum NodeType
	{
		File,
		Folder,
		JSFile,
		Code
	}

	public class MyTreeNode : TreeNode
	{
		public String ThePath { get; set; }
		public NodeType NodeType { get; set; }
		public bool isLoaded { get; set; }
		public int Start { get; set; }
		public int End { get; set; }
	}


}
