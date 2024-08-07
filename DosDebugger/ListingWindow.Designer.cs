﻿namespace DosDebugger
{
    partial class ListingWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ListingWindow));
            this.mnuListing = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuListingOutgoingXRefs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuListingIncomingXRefs = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cbSegments = new System.Windows.Forms.ComboBox();
            this.cbProcedures = new System.Windows.Forms.ComboBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.btnViewScope = new System.Windows.Forms.ToolStripSplitButton();
            this.mnuScopeProcedure = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScopeSegment = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScopeExecutable = new System.Windows.Forms.ToolStripMenuItem();
            this.txtStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lvListing = new Util.Forms.DoubleBufferedListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Address = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewLinkColumn();
            this.mnuListing.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // mnuListing
            // 
            this.mnuListing.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuListingOutgoingXRefs,
            this.toolStripMenuItem1,
            this.mnuListingIncomingXRefs});
            this.mnuListing.Name = "contextMenuListing";
            this.mnuListing.Size = new System.Drawing.Size(242, 58);
            this.mnuListing.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.contextMenuListing_Closed);
            this.mnuListing.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuListing_Opening);
            // 
            // mnuListingOutgoingXRefs
            // 
            this.mnuListingOutgoingXRefs.Enabled = false;
            this.mnuListingOutgoingXRefs.Image = global::DosDebugger.Properties.Resources.OutgoingLink;
            this.mnuListingOutgoingXRefs.Name = "mnuListingOutgoingXRefs";
            this.mnuListingOutgoingXRefs.Size = new System.Drawing.Size(241, 24);
            this.mnuListingOutgoingXRefs.Text = "? XRefs From This Location";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(238, 6);
            // 
            // mnuListingIncomingXRefs
            // 
            this.mnuListingIncomingXRefs.Enabled = false;
            this.mnuListingIncomingXRefs.Image = global::DosDebugger.Properties.Resources.IncomingLink;
            this.mnuListingIncomingXRefs.Name = "mnuListingIncomingXRefs";
            this.mnuListingIncomingXRefs.Size = new System.Drawing.Size(241, 24);
            this.mnuListingIncomingXRefs.Text = "? XRefs To This Location";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.cbSegments, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lvListing, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.cbProcedures, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridView1, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(719, 315);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // cbSegments
            // 
            this.cbSegments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbSegments.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSegments.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbSegments.FormattingEnabled = true;
            this.cbSegments.Location = new System.Drawing.Point(1, 1);
            this.cbSegments.Margin = new System.Windows.Forms.Padding(1);
            this.cbSegments.Name = "cbSegments";
            this.cbSegments.Size = new System.Drawing.Size(357, 27);
            this.cbSegments.TabIndex = 1;
            // 
            // cbProcedures
            // 
            this.cbProcedures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbProcedures.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProcedures.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbProcedures.FormattingEnabled = true;
            this.cbProcedures.Location = new System.Drawing.Point(360, 1);
            this.cbProcedures.Margin = new System.Windows.Forms.Padding(1);
            this.cbProcedures.Name = "cbProcedures";
            this.cbProcedures.Size = new System.Drawing.Size(358, 27);
            this.cbProcedures.TabIndex = 2;
            this.cbProcedures.SelectedIndexChanged += new System.EventHandler(this.cbProcedures_SelectedIndexChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnViewScope,
            this.txtStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 315);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(719, 25);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // btnViewScope
            // 
            this.btnViewScope.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnViewScope.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuScopeProcedure,
            this.mnuScopeSegment,
            this.mnuScopeExecutable});
            this.btnViewScope.Image = ((System.Drawing.Image)(resources.GetObject("btnViewScope.Image")));
            this.btnViewScope.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnViewScope.Name = "btnViewScope";
            this.btnViewScope.Size = new System.Drawing.Size(61, 23);
            this.btnViewScope.Text = "Scope";
            this.btnViewScope.DropDownOpening += new System.EventHandler(this.btnViewScope_DropDownOpening);
            // 
            // mnuScopeProcedure
            // 
            this.mnuScopeProcedure.Name = "mnuScopeProcedure";
            this.mnuScopeProcedure.Size = new System.Drawing.Size(214, 24);
            this.mnuScopeProcedure.Text = "View &Procedure";
            this.mnuScopeProcedure.Click += new System.EventHandler(this.mnuScopeProcedure_Click);
            // 
            // mnuScopeSegment
            // 
            this.mnuScopeSegment.Name = "mnuScopeSegment";
            this.mnuScopeSegment.Size = new System.Drawing.Size(214, 24);
            this.mnuScopeSegment.Text = "View &Segment";
            this.mnuScopeSegment.Click += new System.EventHandler(this.mnuScopeSegment_Click);
            // 
            // mnuScopeExecutable
            // 
            this.mnuScopeExecutable.Name = "mnuScopeExecutable";
            this.mnuScopeExecutable.Size = new System.Drawing.Size(214, 24);
            this.mnuScopeExecutable.Text = "View &Entire Executable";
            this.mnuScopeExecutable.Click += new System.EventHandler(this.mnuScopeExecutable_Click);
            // 
            // txtStatus
            // 
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(71, 20);
            this.txtStatus.Text = "(Message)";
            // 
            // lvListing
            // 
            this.lvListing.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvListing.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lvListing.ContextMenuStrip = this.mnuListing;
            this.lvListing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvListing.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvListing.FullRowSelect = true;
            this.lvListing.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvListing.HideSelection = false;
            this.lvListing.Location = new System.Drawing.Point(1, 51);
            this.lvListing.Margin = new System.Windows.Forms.Padding(1);
            this.lvListing.MultiSelect = false;
            this.lvListing.Name = "lvListing";
            this.lvListing.OwnerDraw = true;
            this.lvListing.Size = new System.Drawing.Size(357, 263);
            this.lvListing.TabIndex = 0;
            this.lvListing.UseCompatibleStateImageBehavior = false;
            this.lvListing.View = System.Windows.Forms.View.Details;
            this.lvListing.VirtualMode = true;
            this.lvListing.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.lvListing_DrawColumnHeader);
            this.lvListing.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.lvListing_DrawItem);
            this.lvListing.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.lvListing_DrawSubItem);
            this.lvListing.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.lvListing_RetrieveVirtualItem);
            this.lvListing.SelectedIndexChanged += new System.EventHandler(this.lvListing_SelectedIndexChanged);
            this.lvListing.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lvListing_MouseMove);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Address";
            this.columnHeader1.Width = 90;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Opcode";
            this.columnHeader2.Width = 160;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Disassembly";
            this.columnHeader3.Width = 380;
            // 
            // linkLabel1
            // 
            this.linkLabel1.BackColor = System.Drawing.Color.Transparent;
            this.linkLabel1.Location = new System.Drawing.Point(301, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(145, 36);
            this.linkLabel1.TabIndex = 6;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "call _some_func";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Address,
            this.Column1,
            this.Column2});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView1.Location = new System.Drawing.Point(362, 53);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(354, 259);
            this.dataGridView1.TabIndex = 3;
            // 
            // Address
            // 
            this.Address.DataPropertyName = "Location";
            this.Address.HeaderText = "Address";
            this.Address.Name = "Address";
            this.Address.ReadOnly = true;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Opcode";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.DataPropertyName = "Text";
            this.Column2.HeaderText = "Disassembly";
            this.Column2.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Column2.TrackVisitedState = false;
            this.Column2.Width = 200;
            // 
            // ListingWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 340);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ListingWindow";
            this.Text = "Disassembly";
            this.Load += new System.EventHandler(this.ListingWindow_Load);
            this.mnuListing.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Util.Forms.DoubleBufferedListView lvListing;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ContextMenuStrip mnuListing;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox cbSegments;
        private System.Windows.Forms.ComboBox cbProcedures;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripSplitButton btnViewScope;
        private System.Windows.Forms.ToolStripStatusLabel txtStatus;
        private System.Windows.Forms.ToolStripMenuItem mnuScopeProcedure;
        private System.Windows.Forms.ToolStripMenuItem mnuScopeSegment;
        private System.Windows.Forms.ToolStripMenuItem mnuScopeExecutable;
        private System.Windows.Forms.ToolStripMenuItem mnuListingOutgoingXRefs;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuListingIncomingXRefs;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Address;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewLinkColumn Column2;
    }
}