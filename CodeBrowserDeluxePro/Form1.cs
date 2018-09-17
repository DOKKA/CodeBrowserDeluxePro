using Newtonsoft.Json;
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


namespace CodeBrowserDeluxePro
{
	public partial class Form1 : Form
	{
		private ScintillaNET.Scintilla TextArea;
		private HttpClient client;
		public Form1()
		{
			InitializeComponent();
		}

		static bool IsJSFile(string f)
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
				NodeType = IsJSFile(f) ? NodeType.JSFile : NodeType.File,
				ThePath = f
			});

			var folders = Directory.GetDirectories(path).Select(d => new MyTreeNode
			{
				Text = Path.GetFileName(d),
				NodeType = NodeType.Folder,
				ThePath = d
			});

			List<MyTreeNode> list = new List<MyTreeNode>();
			list.AddRange(files);
			list.AddRange(folders);
			return list.ToArray();
		}
		
		private async Task<string> doThingAsync(string ThePath)
		{
			var values = new Dictionary<string, string>
			{
			   { "fileName", ThePath }
			};

			var content = new FormUrlEncodedContent(values);

			var response = await client.PostAsync("http://localhost:3000/parse", content);

			return await response.Content.ReadAsStringAsync();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			TextArea = new ScintillaNET.Scintilla();
			splitContainer1.Panel2.Controls.Add(TextArea);

			// BASIC CONFIG
			TextArea.Dock = System.Windows.Forms.DockStyle.Fill;
			client  = new HttpClient();
			//tvFiles.Nodes.AddRange()
			string workspace = @"C:\Users\kevin\code";
			tvFiles.Nodes.AddRange(GetNodes(workspace));

		}

		private async void tvFiles_AfterSelect(object sender, TreeViewEventArgs e)
		{
			var node = (MyTreeNode)e.Node;
			if(node.NodeType == NodeType.Folder && !node.isLoaded)
			{
				tvFiles.SelectedNode.Nodes.AddRange(GetNodes(node.ThePath));
				node.isLoaded = true;
			}
			if(node.NodeType == NodeType.JSFile && !node.isLoaded)
			{
				try
				{
					string resp = await doThingAsync(node.ThePath);
					tvFiles.SelectedNode.Nodes.AddRange(GetCodeNodes(resp));
					node.isLoaded = true;
				}
				catch (Exception ex)
				{

				}
				

			}
			if(node.NodeType == NodeType.Code)
			{
				string fileText = File.ReadAllText(((MyTreeNode)node.Parent).ThePath);
				string  shit = fileText.Substring(node.Start, node.End - node.Start);
				TextArea.Text = shit;
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
