using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBrowserDeluxePro
{
	public enum Syntax
	{
		JAVASCRIPT,
		JSON,
		CPP,
		HTML,
		XML,
		CSS,
		LESS,
		SASS,
		JSX,
		TS,
		TXT
	}

	class ScintillaHelper
	{

		public static Color IntToColor(int rgb)
		{
			return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
		}

		private bool isMarginClickBound = false;
		/// <summary>
		/// the background color of the text area
		/// </summary>
		private const int BACK_COLOR = 0x2A211C;

		/// <summary>
		/// default text color of the text area
		/// </summary>
		private const int FORE_COLOR = 0xB7B7B7;

		/// <summary>
		/// change this to whatever margin you want the line numbers to show in
		/// </summary>
		private const int NUMBER_MARGIN = 1;

		/// <summary>
		/// change this to whatever margin you want the bookmarks/breakpoints to show in
		/// </summary>
		private const int BOOKMARK_MARGIN = 2;
		private const int BOOKMARK_MARKER = 2;

		/// <summary>
		/// change this to whatever margin you want the code folding tree (+/-) to show in
		/// </summary>
		private const int FOLDING_MARGIN = 3;

		/// <summary>
		/// set this true to show circular buttons for code folding (the [+] and [-] buttons on the margin)
		/// </summary>
		private const bool CODEFOLDING_CIRCULAR = true;

		private string Default_Font = "Consolas";

		public ScintillaNET.Scintilla TextArea { get; set; }


		private bool IsFontInstalled(string fontName)
		{
			using (var testFont = new Font(fontName, 10))
			{
				return 0 == string.Compare(
				fontName,
				testFont.Name,
				StringComparison.InvariantCultureIgnoreCase);
			}
		}

		public void Init(Syntax syntax)
		{

			if (IsFontInstalled("Source Code Pro"))
			{
				Default_Font = "Source Code Pro";
			}
			// INITIAL VIEW CONFIG
			TextArea.WrapMode = WrapMode.None;
			TextArea.IndentationGuides = IndentView.LookBoth;

			// STYLING
			InitColors();
			if(syntax == Syntax.XML || syntax == Syntax.HTML)
			{
				InitSyntaxColoringXML(syntax);
			} else	{
				InitSyntaxColoring(syntax);
				//InitSyntaxColoringXML(syntax);
			}
			


			// NUMBER MARGIN
			InitNumberMargin();

			// BOOKMARK MARGIN
			InitBookmarkMargin();

			// CODE FOLDING MARGIN
			InitCodeFolding();

		}

		private void InitColors()
		{

			TextArea.SetSelectionBackColor(true, IntToColor(0x114D9C));

		}

		private void InitSyntaxColoring(Syntax syntax)
		{

			// Configure the default style
			TextArea.StyleResetDefault();
			TextArea.Styles[Style.Default].Font = Default_Font;
			TextArea.Styles[Style.Default].Size = 10;
			TextArea.Styles[Style.Default].BackColor = IntToColor(0x212121);
			TextArea.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
			TextArea.CaretForeColor = IntToColor(0xFFFFFF);
			TextArea.CaretLineBackColor = IntToColor(0x2F3142);
			TextArea.CaretLineVisible = true;
			TextArea.StyleClearAll();

			// Configure the CPP (C#) lexer styles
			TextArea.Styles[Style.Cpp.Preprocessor].ForeColor = IntToColor(0xCEAC00);
			TextArea.Styles[Style.Cpp.Identifier].ForeColor = IntToColor(0xE8E8E8);
			TextArea.Styles[Style.Cpp.Word].ForeColor = IntToColor(0xC5A3FF);
			TextArea.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0x6EB5FF);
			TextArea.Styles[Style.Cpp.Number].ForeColor = IntToColor(0x55FFFC);
			TextArea.Styles[Style.Cpp.String].ForeColor = IntToColor(0xFF5555);
			TextArea.Styles[Style.Cpp.Character].ForeColor = IntToColor(0xFF9205);
			TextArea.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xFFFF00);
			TextArea.Styles[Style.Cpp.Verbatim].ForeColor = IntToColor(0xFF9205);
			TextArea.Styles[Style.Cpp.Regex].ForeColor = IntToColor(0x8BE9FD);
			TextArea.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0x50FA7B);
			TextArea.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x50FA7B);
			TextArea.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x50FA7B);
			TextArea.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x50FA7B);
			TextArea.Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0x50FA7B);
			TextArea.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0x50FA7B);
			TextArea.Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0xFFFFFF);

			TextArea.Lexer = Lexer.Cpp;

			TextArea.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
			TextArea.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");

		}


		private void InitSyntaxColoringXML(Syntax syntax)
		{
			// Configure the default style
			TextArea.StyleResetDefault();
			TextArea.Styles[Style.Default].Font = Default_Font;
			TextArea.Styles[Style.Default].Size = 10;
			TextArea.Styles[Style.Default].BackColor = IntToColor(0x212121);
			TextArea.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
			TextArea.CaretForeColor = IntToColor(0xFFFFFF);
			TextArea.CaretLineBackColor = IntToColor(0x2F3142);
			TextArea.CaretLineVisible = true;
			TextArea.StyleClearAll();

			TextArea.Styles[Style.Xml.Attribute].ForeColor = IntToColor(0xFF5555);
			TextArea.Styles[Style.Xml.Entity].ForeColor = IntToColor(0xC5A3FF);
			TextArea.Styles[Style.Xml.Comment].ForeColor = IntToColor(0x50FA7B);
			TextArea.Styles[Style.Xml.Tag].ForeColor = IntToColor(0xCEAC00);
			TextArea.Styles[Style.Xml.TagEnd].ForeColor = IntToColor(0xCEAC00);
			TextArea.Styles[Style.Xml.DoubleString].ForeColor = IntToColor(0xFF5555);
			TextArea.Styles[Style.Xml.SingleString].ForeColor = IntToColor(0xFF9205);

			TextArea.Lexer = Lexer.Html;
		}


		private void InitNumberMargin()
		{

			TextArea.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
			TextArea.Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
			TextArea.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
			TextArea.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

			var nums = TextArea.Margins[NUMBER_MARGIN];
			nums.Width = 30;
			nums.Type = MarginType.Number;
			nums.Sensitive = true;
			nums.Mask = 0;
			if (!isMarginClickBound)
			{
				TextArea.MarginClick += TextArea_MarginClick;
			}
			
		}

		private void InitBookmarkMargin()
		{

			//TextArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));

			var margin = TextArea.Margins[BOOKMARK_MARGIN];
			margin.Width = 20;
			margin.Sensitive = true;
			margin.Type = MarginType.Symbol;
			margin.Mask = (1 << BOOKMARK_MARKER);
			//margin.Cursor = MarginCursor.Arrow;

			var marker = TextArea.Markers[BOOKMARK_MARKER];
			marker.Symbol = MarkerSymbol.Circle;
			marker.SetBackColor(IntToColor(0xFF003B));
			marker.SetForeColor(IntToColor(0x000000));
			marker.SetAlpha(100);

		}

		private void InitCodeFolding()
		{

			TextArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));
			TextArea.SetFoldMarginHighlightColor(true, IntToColor(BACK_COLOR));

			// Enable code folding
			TextArea.SetProperty("fold", "1");
			TextArea.SetProperty("fold.compact", "1");

			// Configure a margin to display folding symbols
			TextArea.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
			TextArea.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
			TextArea.Margins[FOLDING_MARGIN].Sensitive = true;
			TextArea.Margins[FOLDING_MARGIN].Width = 20;

			// Set colors for all folding markers
			for (int i = 25; i <= 31; i++)
			{
				TextArea.Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
				TextArea.Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
			}

			// Configure folding markers with respective symbols
			TextArea.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
			TextArea.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
			TextArea.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
			TextArea.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
			TextArea.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
			TextArea.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
			TextArea.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

			// Enable automatic folding
			TextArea.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

		}

		private void TextArea_MarginClick(object sender, MarginClickEventArgs e)
		{
			if (e.Margin == BOOKMARK_MARGIN)
			{
				// Do we have a marker for this line?
				const uint mask = (1 << BOOKMARK_MARKER);
				var line = TextArea.Lines[TextArea.LineFromPosition(e.Position)];
				if ((line.MarkerGet() & mask) > 0)
				{
					// Remove existing bookmark
					line.MarkerDelete(BOOKMARK_MARKER);
				}
				else
				{
					// Add bookmark
					line.MarkerAdd(BOOKMARK_MARKER);
				}
			}
		}


	}
}
