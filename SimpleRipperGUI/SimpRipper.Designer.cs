namespace SimpRipper
{
    partial class SimpRipper
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimpRipper));
			this.infoBox = new System.Windows.Forms.GroupBox();
			this.authorBox = new System.Windows.Forms.GroupBox();
			this.authorID = new System.Windows.Forms.Label();
			this.authorName = new System.Windows.Forms.Label();
			this.authorLinkLabel = new System.Windows.Forms.LinkLabel();
			this.autorIDLabel = new System.Windows.Forms.Label();
			this.authorNameLabel = new System.Windows.Forms.Label();
			this.avatarBox = new System.Windows.Forms.GroupBox();
			this.avatarReleaseStatus = new System.Windows.Forms.Label();
			this.avatarDescription = new System.Windows.Forms.Label();
			this.avatarID = new System.Windows.Forms.Label();
			this.avatarName = new System.Windows.Forms.Label();
			this.avatarIDLabel = new System.Windows.Forms.Label();
			this.avatarNameLabel = new System.Windows.Forms.Label();
			this.avatarReleaseStatusLabel = new System.Windows.Forms.Label();
			this.avatarDescriptionLabel = new System.Windows.Forms.Label();
			this.avatarImageBox = new System.Windows.Forms.PictureBox();
			this.apiKeyLabel = new System.Windows.Forms.Label();
			this.loginBox = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.apiKeyBox = new System.Windows.Forms.TextBox();
			this.avatarIDBox = new System.Windows.Forms.TextBox();
			this.infoBox.SuspendLayout();
			this.authorBox.SuspendLayout();
			this.avatarBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.avatarImageBox)).BeginInit();
			this.loginBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// infoBox
			// 
			this.infoBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.infoBox.AutoSize = true;
			this.infoBox.Controls.Add(this.authorBox);
			this.infoBox.Controls.Add(this.avatarBox);
			this.infoBox.Location = new System.Drawing.Point(458, 107);
			this.infoBox.Name = "infoBox";
			this.infoBox.Size = new System.Drawing.Size(590, 341);
			this.infoBox.TabIndex = 1;
			this.infoBox.TabStop = false;
			this.infoBox.Text = "Information";
			// 
			// authorBox
			// 
			this.authorBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.authorBox.AutoSize = true;
			this.authorBox.Controls.Add(this.authorID);
			this.authorBox.Controls.Add(this.authorName);
			this.authorBox.Controls.Add(this.authorLinkLabel);
			this.authorBox.Controls.Add(this.autorIDLabel);
			this.authorBox.Controls.Add(this.authorNameLabel);
			this.authorBox.Location = new System.Drawing.Point(10, 217);
			this.authorBox.Name = "authorBox";
			this.authorBox.Size = new System.Drawing.Size(574, 102);
			this.authorBox.TabIndex = 6;
			this.authorBox.TabStop = false;
			this.authorBox.Text = "Author";
			// 
			// authorID
			// 
			this.authorID.AutoSize = true;
			this.authorID.Location = new System.Drawing.Point(91, 41);
			this.authorID.Name = "authorID";
			this.authorID.Size = new System.Drawing.Size(0, 13);
			this.authorID.TabIndex = 13;
			// 
			// authorName
			// 
			this.authorName.AutoSize = true;
			this.authorName.Location = new System.Drawing.Point(91, 16);
			this.authorName.Name = "authorName";
			this.authorName.Size = new System.Drawing.Size(0, 13);
			this.authorName.TabIndex = 12;
			// 
			// authorLinkLabel
			// 
			this.authorLinkLabel.AutoSize = true;
			this.authorLinkLabel.Location = new System.Drawing.Point(9, 73);
			this.authorLinkLabel.Name = "authorLinkLabel";
			this.authorLinkLabel.Size = new System.Drawing.Size(57, 13);
			this.authorLinkLabel.TabIndex = 4;
			this.authorLinkLabel.TabStop = true;
			this.authorLinkLabel.Text = "Author link";
			this.authorLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AuthorLinkLabel_LinkClicked);
			// 
			// autorIDLabel
			// 
			this.autorIDLabel.AutoSize = true;
			this.autorIDLabel.Location = new System.Drawing.Point(6, 41);
			this.autorIDLabel.Name = "autorIDLabel";
			this.autorIDLabel.Size = new System.Drawing.Size(18, 13);
			this.autorIDLabel.TabIndex = 3;
			this.autorIDLabel.Text = "ID";
			// 
			// authorNameLabel
			// 
			this.authorNameLabel.AutoSize = true;
			this.authorNameLabel.Location = new System.Drawing.Point(6, 16);
			this.authorNameLabel.Name = "authorNameLabel";
			this.authorNameLabel.Size = new System.Drawing.Size(35, 13);
			this.authorNameLabel.TabIndex = 2;
			this.authorNameLabel.Text = "Name";
			// 
			// avatarBox
			// 
			this.avatarBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.avatarBox.AutoSize = true;
			this.avatarBox.Controls.Add(this.avatarReleaseStatus);
			this.avatarBox.Controls.Add(this.avatarDescription);
			this.avatarBox.Controls.Add(this.avatarID);
			this.avatarBox.Controls.Add(this.avatarName);
			this.avatarBox.Controls.Add(this.avatarIDLabel);
			this.avatarBox.Controls.Add(this.avatarNameLabel);
			this.avatarBox.Controls.Add(this.avatarReleaseStatusLabel);
			this.avatarBox.Controls.Add(this.avatarDescriptionLabel);
			this.avatarBox.Location = new System.Drawing.Point(10, 19);
			this.avatarBox.Name = "avatarBox";
			this.avatarBox.Size = new System.Drawing.Size(574, 189);
			this.avatarBox.TabIndex = 5;
			this.avatarBox.TabStop = false;
			this.avatarBox.Text = "Avatar";
			// 
			// avatarReleaseStatus
			// 
			this.avatarReleaseStatus.AutoSize = true;
			this.avatarReleaseStatus.Location = new System.Drawing.Point(91, 138);
			this.avatarReleaseStatus.Name = "avatarReleaseStatus";
			this.avatarReleaseStatus.Size = new System.Drawing.Size(0, 13);
			this.avatarReleaseStatus.TabIndex = 11;
			// 
			// avatarDescription
			// 
			this.avatarDescription.AutoSize = true;
			this.avatarDescription.Location = new System.Drawing.Point(91, 103);
			this.avatarDescription.Name = "avatarDescription";
			this.avatarDescription.Size = new System.Drawing.Size(0, 13);
			this.avatarDescription.TabIndex = 10;
			// 
			// avatarID
			// 
			this.avatarID.AutoSize = true;
			this.avatarID.Location = new System.Drawing.Point(88, 63);
			this.avatarID.Name = "avatarID";
			this.avatarID.Size = new System.Drawing.Size(0, 13);
			this.avatarID.TabIndex = 9;
			// 
			// avatarName
			// 
			this.avatarName.AutoSize = true;
			this.avatarName.Location = new System.Drawing.Point(88, 29);
			this.avatarName.Name = "avatarName";
			this.avatarName.Size = new System.Drawing.Size(0, 13);
			this.avatarName.TabIndex = 8;
			// 
			// avatarIDLabel
			// 
			this.avatarIDLabel.AutoSize = true;
			this.avatarIDLabel.Location = new System.Drawing.Point(6, 63);
			this.avatarIDLabel.Name = "avatarIDLabel";
			this.avatarIDLabel.Size = new System.Drawing.Size(18, 13);
			this.avatarIDLabel.TabIndex = 6;
			this.avatarIDLabel.Text = "ID";
			// 
			// avatarNameLabel
			// 
			this.avatarNameLabel.AutoSize = true;
			this.avatarNameLabel.Location = new System.Drawing.Point(6, 29);
			this.avatarNameLabel.Name = "avatarNameLabel";
			this.avatarNameLabel.Size = new System.Drawing.Size(35, 13);
			this.avatarNameLabel.TabIndex = 5;
			this.avatarNameLabel.Text = "Name";
			// 
			// avatarReleaseStatusLabel
			// 
			this.avatarReleaseStatusLabel.AutoSize = true;
			this.avatarReleaseStatusLabel.Location = new System.Drawing.Point(6, 138);
			this.avatarReleaseStatusLabel.Name = "avatarReleaseStatusLabel";
			this.avatarReleaseStatusLabel.Size = new System.Drawing.Size(79, 13);
			this.avatarReleaseStatusLabel.TabIndex = 3;
			this.avatarReleaseStatusLabel.Text = "Release Status";
			// 
			// avatarDescriptionLabel
			// 
			this.avatarDescriptionLabel.AutoSize = true;
			this.avatarDescriptionLabel.Location = new System.Drawing.Point(6, 103);
			this.avatarDescriptionLabel.Name = "avatarDescriptionLabel";
			this.avatarDescriptionLabel.Size = new System.Drawing.Size(60, 13);
			this.avatarDescriptionLabel.TabIndex = 4;
			this.avatarDescriptionLabel.Text = "Description";
			// 
			// avatarImageBox
			// 
			this.avatarImageBox.Location = new System.Drawing.Point(2, 1);
			this.avatarImageBox.Name = "avatarImageBox";
			this.avatarImageBox.Size = new System.Drawing.Size(450, 450);
			this.avatarImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.avatarImageBox.TabIndex = 0;
			this.avatarImageBox.TabStop = false;
			// 
			// apiKeyLabel
			// 
			this.apiKeyLabel.AutoSize = true;
			this.apiKeyLabel.Location = new System.Drawing.Point(6, 27);
			this.apiKeyLabel.Margin = new System.Windows.Forms.Padding(3);
			this.apiKeyLabel.Name = "apiKeyLabel";
			this.apiKeyLabel.Size = new System.Drawing.Size(39, 13);
			this.apiKeyLabel.TabIndex = 3;
			this.apiKeyLabel.Text = "apiKey";
			// 
			// loginBox
			// 
			this.loginBox.Controls.Add(this.label1);
			this.loginBox.Controls.Add(this.apiKeyBox);
			this.loginBox.Controls.Add(this.avatarIDBox);
			this.loginBox.Controls.Add(this.apiKeyLabel);
			this.loginBox.Location = new System.Drawing.Point(458, 12);
			this.loginBox.Name = "loginBox";
			this.loginBox.Size = new System.Drawing.Size(584, 92);
			this.loginBox.TabIndex = 5;
			this.loginBox.TabStop = false;
			this.loginBox.Text = "User Input";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 55);
			this.label1.Margin = new System.Windows.Forms.Padding(3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 13);
			this.label1.TabIndex = 7;
			this.label1.Text = "avatarID";
			// 
			// apiKeyBox
			// 
			this.apiKeyBox.Location = new System.Drawing.Point(60, 24);
			this.apiKeyBox.Multiline = true;
			this.apiKeyBox.Name = "apiKeyBox";
			this.apiKeyBox.Size = new System.Drawing.Size(518, 20);
			this.apiKeyBox.TabIndex = 5;
			this.apiKeyBox.Text = "JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26";
			// 
			// avatarIDBox
			// 
			this.avatarIDBox.Location = new System.Drawing.Point(60, 52);
			this.avatarIDBox.Name = "avatarIDBox";
			this.avatarIDBox.Size = new System.Drawing.Size(518, 20);
			this.avatarIDBox.TabIndex = 6;
			this.avatarIDBox.TextChanged += new System.EventHandler(this.AvatarIDBox_TextChanged);
			// 
			// SimpRipper
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1060, 450);
			this.Controls.Add(this.loginBox);
			this.Controls.Add(this.infoBox);
			this.Controls.Add(this.avatarImageBox);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SimpRipper";
			this.Text = "VRChat SimpRipper";
			this.infoBox.ResumeLayout(false);
			this.infoBox.PerformLayout();
			this.authorBox.ResumeLayout(false);
			this.authorBox.PerformLayout();
			this.avatarBox.ResumeLayout(false);
			this.avatarBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.avatarImageBox)).EndInit();
			this.loginBox.ResumeLayout(false);
			this.loginBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox infoBox;
        private System.Windows.Forms.GroupBox authorBox;
        private System.Windows.Forms.LinkLabel authorLinkLabel;
        private System.Windows.Forms.Label autorIDLabel;
        private System.Windows.Forms.Label authorNameLabel;
        private System.Windows.Forms.GroupBox avatarBox;
        private System.Windows.Forms.Label avatarIDLabel;
        private System.Windows.Forms.Label avatarNameLabel;
        private System.Windows.Forms.Label avatarReleaseStatusLabel;
        private System.Windows.Forms.Label avatarDescriptionLabel;
        private System.Windows.Forms.PictureBox avatarImageBox;
        private System.Windows.Forms.Label apiKeyLabel;
        private System.Windows.Forms.GroupBox loginBox;
        private System.Windows.Forms.TextBox apiKeyBox;
        private System.Windows.Forms.TextBox avatarIDBox;
        private System.Windows.Forms.Label authorID;
        private System.Windows.Forms.Label authorName;
        private System.Windows.Forms.Label avatarReleaseStatus;
        private System.Windows.Forms.Label avatarDescription;
        private System.Windows.Forms.Label avatarID;
        private System.Windows.Forms.Label avatarName;
        private System.Windows.Forms.Label label1;
    }
}

