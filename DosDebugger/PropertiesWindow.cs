﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Disassembler;

namespace DosDebugger
{
    public partial class PropertiesWindow : ToolWindow
    {
        public PropertiesWindow()
        {
            InitializeComponent();
        }

        public object SelectedObject
        {
            get { return propertyGrid1.SelectedObject; }
            set { propertyGrid1.SelectedObject = value; }
        }

        private void PropertiesWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            //propertyGrid1.SelectedObject = null;
        }
    }
}
