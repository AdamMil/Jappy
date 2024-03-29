/*
Jappy is a Japanese dictionary and study tool.

http://www.adammil.net/
Copyright (C) 2007 Adam Milazzo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Jappy.Backend;

namespace Jappy
{

partial class MainForm : Form
{
  public MainForm()
  {
    InitializeComponent();
  }

  public TabBase SelectedTab
  {
    get { return (TabBase)tabControl.SelectedTab.Controls[0]; }
  }

  public DictionarySearchTab GetDictionarySearchTab()
  {
    return (DictionarySearchTab)dictionaryPage.Controls[0];
  }

  public ExampleSearchTab GetExampleSearchTab()
  {
    return (ExampleSearchTab)examplePage.Controls[0];
  }

  public StudyTab GetStudyTab()
  {
    return (StudyTab)studyPage.Controls[0];
  }

  public TranslateTab GetTranslationTab()
  {
    return (TranslateTab)translatePage.Controls[0];
  }

  public void SwitchToTab(TabPage page)
  {
    tabControl.SelectedTab = page;
  }

  protected override void OnActivated(EventArgs e)
  {
    base.OnActivated(e);
    isActive = true;
  }

  protected override void OnDeactivate(EventArgs e)
  {
    base.OnDeactivate(e);
    isActive = false;
  }

  protected override void OnLoad(EventArgs e)
  {
    base.OnLoad(e);
    AddGlobalHotkey();
  }

  protected override void OnClosing(CancelEventArgs e)
  {
    if(!GetStudyTab().TryCloseList()) e.Cancel = true;
  }

  protected override void OnClosed(EventArgs e)
  {
    base.OnClosed(e);
    RemoveGlobalHotkey();
  }

  protected override void OnKeyDown(KeyEventArgs e)
  {
    base.OnKeyDown(e);
    if(e.Modifiers != Keys.None) return;

    if(e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F9)
    {
      int page = e.KeyCode - Keys.F1;
      if(page < tabControl.TabPages.Count) tabControl.SelectedTab = tabControl.TabPages[page];
      e.SuppressKeyPress = true;
    }
  }

  protected override void OnSizeChanged(EventArgs e)
  {
    base.OnSizeChanged(e);
    
    if(WindowState == FormWindowState.Minimized)
    {
      this.Hide();
      this.notifyIcon.Visible = true;
    }
    else
    {
      this.Show();
      this.notifyIcon.Visible = false;
    }
  }

  protected override void WndProc(ref Message m)
  {
    base.WndProc(ref m);
    
    const int WM_HOTKEY = 0x312;
    if(m.Msg == WM_HOTKEY) OnGlobalHotkeyPressed();
  }

  #region Event handlers
  private void exitMenuItem_Click(object sender, EventArgs e)
  {
    Close();
  }

  void findMenuItem_Click(object sender, EventArgs e)
  {
    ShowFinder(SelectedTab.OutputArea);
  }

  void findAgainMenuItem_Click(object sender, EventArgs e)
  {
    FindNext(SelectedTab.OutputArea);
  }

  void viewMenuItem_DropDownOpening(object sender, EventArgs e)
  {
    findAgainMenuItem.Enabled = HasFinderText;
  }

  void tabControl_Deselecting(object sender, TabControlCancelEventArgs e)
  {
    TabBase tab = (TabBase)e.TabPage.Controls[0];
    tab.OnDeactivate();
  }

  void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
  {
    TabBase tab = (TabBase)e.TabPage.Controls[0];
    if(finder != null) finder.Document = tab.OutputArea;
    tab.OnActivate();
  }

  void notifyIcon_Click(object sender, EventArgs e)
  {
    OnGlobalHotkeyPressed();
  }
  #endregion

  #region Global hot keys
  void AddGlobalHotkey()
  {
    if(globalKeyAtom != 0) RemoveGlobalHotkey();

    string atomName = "Jappy"+System.Diagnostics.Process.GetCurrentProcess().Id;
    globalKeyAtom = GlobalAddAtom(atomName);

    if(globalKeyAtom != 0) // if we could create the atom, register the key
    {
      if(RegisterHotKey(Handle, globalKeyAtom, MOD_WIN, (int)Keys.G) == 0)
      {
        GlobalDeleteAtom(globalKeyAtom); // if we couldn't register the key, delete the atom
        globalKeyAtom = 0;
      }
    }
  }

  void RemoveGlobalHotkey()
  {
    if(globalKeyAtom != 0)
    {
      UnregisterHotKey(Handle, globalKeyAtom);
      GlobalDeleteAtom(globalKeyAtom);
    }
  }
  
  void OnGlobalHotkeyPressed()
  {
    if(!isActive || !Visible || WindowState == FormWindowState.Minimized)
    {
      Show();
      if(WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
      Activate();
      /*dictInput.Focus();
      dictInput.SelectAll();*/
    }
    else
    {
      WindowState = FormWindowState.Minimized;
    }
  }

  const int MOD_ALT = 1, MOD_CONTROL = 2, MOD_SHIFT = 4, MOD_WIN = 8;
  [System.Runtime.InteropServices.DllImport("user32")]
  static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);
  [System.Runtime.InteropServices.DllImport("user32")]
  static extern int UnregisterHotKey(IntPtr hwnd, int id);
  [System.Runtime.InteropServices.DllImport("kernel32")]
  static extern short GlobalAddAtom(string lpString);
  [System.Runtime.InteropServices.DllImport("kernel32")]
  static extern short GlobalDeleteAtom(short nAtom);
  #endregion

  #region Status text handling
  sealed class StatusText
  {
    public StatusText(Control control)
    {
      this.control = new WeakReference(control);
    }
    
    public Control Control
    {
      get { return (Control)control.Target; }
    }
    
    public string Text;

    readonly WeakReference control;
  }
  
  internal void ShowStatusText(Control control)
  {
    for(int i=0; i<statusTexts.Count; i++)
    {
      if(statusTexts[i].Control == control)
      {
        StatusText status = statusTexts[i];
        statusText.Text = status.Text;
        statusTexts.RemoveAt(i);
        statusTexts.Add(status);
        break;
      }
    }
  }

  internal void SetStatusText(string text)
  {
    statusText.Text = text;
  }

  internal void SetStatusText(Control control, string text)
  {
    StatusText status = null;
    for(int i=0; i<statusTexts.Count; i++)
    {
      if(statusTexts[i].Control == control)
      {
        status = statusTexts[i];
        statusTexts.RemoveAt(i);
        break;
      }
    }

    if(status == null) status = new StatusText(control);
    statusText.Text = status.Text = text;
    statusTexts.Add(status);
  }
  
  internal void RestoreStatusText(Control control)
  {
    for(int i=statusTexts.Count-1; i >= 0; i--)
    {
      if(statusTexts[i].Control == control)
      {
        statusTexts.RemoveAt(i);
      }
    }

    statusText.Text = statusTexts.Count == 0 ? "Welcome to Jappy!" : statusTexts[statusTexts.Count-1].Text;
  }
  #endregion

  internal void ShowToolTip(Control control, string title, string text)
  {
    toolTip.ToolTipTitle = title;
    toolTip.Show(text, control, 50, 0, 3000);
  }

  bool HasFinderText
  {
    get { return finder != null && !string.IsNullOrEmpty(finder.SearchText); }
  }

  void FindNext(DocumentRenderer renderer)
  {
    if(!HasFinderText || finder.Document != renderer) ShowFinder(renderer);
    else finder.FindNext();
  }

  void ShowFinder(DocumentRenderer renderer)
  {
    if(finder == null)
    {
      finder = new FindDialog();
      finder.FormClosed += delegate { finder = null; };
      finder.Location = new Point(Right - finder.Width, Top+32);
    }

    finder.Document = renderer;

    if(renderer == null)
    {
      finder.Hide();
    }
    else
    {
      if(renderer.SelectionLength != 0) finder.SearchText = renderer.SelectedText;
      finder.Show();
      finder.Focus();
    }
  }

  readonly List<StatusText> statusTexts = new List<StatusText>();
  FindDialog finder;
  short globalKeyAtom;
  bool isActive;
}

} // namespace Jappy