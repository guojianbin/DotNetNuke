#region Copyright
// 
// DotNetNukeŽ - http://www.dotnetnuke.com
// Copyright (c) 2002-2012
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.WebControls
{

	public class RolesSelectionGrid : Control, INamingContainer
	{

		#region Private Members

		private readonly DataTable _dtRoleSelections = new DataTable();
		private ArrayList _roles;
		private ArrayList _selectedRoles;
		private DropDownList cboRoleGroups;
		private DataGrid dgRoleSelection;
		private Label lblGroups;
		private Panel pnlRoleSlections;
		
		#endregion

		#region Public Properties

		#region DataGrid Properties


		public TableItemStyle AlternatingItemStyle
		{
			get
			{
				return dgRoleSelection.AlternatingItemStyle;
			}
		}

		public bool AutoGenerateColumns
		{
			get
			{
				return dgRoleSelection.AutoGenerateColumns;
			}
			set
			{
				dgRoleSelection.AutoGenerateColumns = value;
			}
		}

		public int CellSpacing
		{
			get
			{
				return dgRoleSelection.CellSpacing;
			}
			set
			{
				dgRoleSelection.CellSpacing = value;
			}
		}

		public DataGridColumnCollection Columns
		{
			get
			{
				return dgRoleSelection.Columns;
			}
		}

		public TableItemStyle FooterStyle
		{
			get
			{
				return dgRoleSelection.FooterStyle;
			}
		}

		public GridLines GridLines
		{
			get
			{
				return dgRoleSelection.GridLines;
			}
			set
			{
				dgRoleSelection.GridLines = value;
			}
		}

		public TableItemStyle HeaderStyle
		{
			get
			{
				return dgRoleSelection.HeaderStyle;
			}
		}

		public TableItemStyle ItemStyle
		{
			get
			{
				return dgRoleSelection.ItemStyle;
			}
		}

		public DataGridItemCollection Items
		{
			get
			{
				return dgRoleSelection.Items;
			}
		}

		public TableItemStyle SelectedItemStyle
		{
			get
			{
				return dgRoleSelection.SelectedItemStyle;
			}
		}
		
		#endregion

		/// <summary>
		/// List of the Names of the selected Roles
		/// </summary>
		public ArrayList SelectedRoleNames
		{
			get
			{
				UpdateRoleSelections();
				return (CurrentRoleSelection);
			}
			set
			{
				CurrentRoleSelection = value;
			}
		}

		 /// <summary>
		 /// Gets and Sets the ResourceFile to localize permissions
		 /// </summary>
		public string ResourceFile { get; set; }

		/// <summary>
		/// Enable ShowAllUsers to display the virtuell "Unauthenticated Users" role
		/// </summary>
		public bool ShowUnauthenticatedUsers
		{
			get
			{
				if (ViewState["ShowUnauthenticatedUsers"] == null)
				{
					return false;
				}
				else
				{
					return Convert.ToBoolean(ViewState["ShowUnauthenticatedUsers"]);
				}
			}
			set
			{
				ViewState["ShowUnauthenticatedUsers"] = value;
			}
		}

		/// <summary>
		/// Enable ShowAllUsers to display the virtuell "All Users" role
		/// </summary>
		public bool ShowAllUsers
		{
			get
			{
				if (ViewState["ShowAllUsers"] == null)
				{
					return false;
				}
				else
				{
					return Convert.ToBoolean(ViewState["ShowAllUsers"]);
				}
			}
			set
			{
				ViewState["ShowAllUsers"] = value;
			}
		}
		
		#endregion

		#region Private Properties
		
		private DataTable dtRolesSelection
		{
			get
			{
				return _dtRoleSelections;
			}
		}

		private ArrayList Roles
		{
			get
			{
				return _roles;
			}
			set
			{
				_roles = value;
			}
		}

		private ArrayList CurrentRoleSelection
		{
			get
			{
				if (_selectedRoles == null)
				{
					_selectedRoles = new ArrayList();
				}
				return _selectedRoles;
			}
			set
			{
				_selectedRoles = value;
			}
		}
		
		#endregion

		#region Private Methods

		/// <summary>
		/// Bind the data to the controls
		/// </summary>
		private void BindData()
		{
			EnsureChildControls();

			BindRolesGrid();
		}

		/// <summary>
		/// Bind the Roles data to the Grid
		/// </summary>
		private void BindRolesGrid()
		{
			dtRolesSelection.Columns.Clear();
			dtRolesSelection.Rows.Clear();

			DataColumn col;

			//Add Roles Column
			col = new DataColumn("RoleId", typeof (string));
			dtRolesSelection.Columns.Add(col);

			//Add Roles Column
			col = new DataColumn("RoleName", typeof (string));
			dtRolesSelection.Columns.Add(col);

			//Add Selected Column
			col = new DataColumn("Selected", typeof (bool));
			dtRolesSelection.Columns.Add(col);

			GetRoles();

			UpdateRoleSelections();
			DataRow row;
			for (int i = 0; i <= Roles.Count - 1; i++)
			{
				var role = (RoleInfo) Roles[i];
				row = dtRolesSelection.NewRow();
				row["RoleId"] = role.RoleID;
				row["RoleName"] = Localization.LocalizeRole(role.RoleName);
				row["Selected"] = GetSelection(role.RoleName);

				dtRolesSelection.Rows.Add(row);
			}
			dgRoleSelection.DataSource = dtRolesSelection;
			dgRoleSelection.DataBind();
		}

		private bool GetSelection(string roleName)
		{
			foreach (string r in CurrentRoleSelection)
			{
				if (r == roleName)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the roles from the Database and loads them into the Roles property
		/// </summary>
		private void GetRoles()
		{
			var objRoleController = new RoleController();
			int RoleGroupId = -2;
			if ((cboRoleGroups != null) && (cboRoleGroups.SelectedValue != null))
			{
				RoleGroupId = int.Parse(cboRoleGroups.SelectedValue);
			}
			if (RoleGroupId > -2)
			{
				_roles = objRoleController.GetRolesByGroup(PortalController.GetCurrentPortalSettings().PortalId, RoleGroupId);
			}
			else
			{
				_roles = objRoleController.GetPortalRoles(PortalController.GetCurrentPortalSettings().PortalId);
			}
			if (RoleGroupId < 0)
			{
				var r = new RoleInfo();
				if (ShowUnauthenticatedUsers)
				{
					r.RoleID = int.Parse(Globals.glbRoleUnauthUser);
					r.RoleName = Globals.glbRoleUnauthUserName;
					_roles.Add(r);
				}
				if (ShowAllUsers)
				{
					r = new RoleInfo();
					r.RoleID = int.Parse(Globals.glbRoleAllUsers);
					r.RoleName = Globals.glbRoleAllUsersName;
					_roles.Add(r);
				}
			}
			_roles.Reverse();
			_roles.Sort(new RoleComparer());
		}

		/// <summary>
		/// Sets up the columns for the Grid
		/// </summary>
		private void SetUpRolesGrid()
		{
			dgRoleSelection.Columns.Clear();
			var textCol = new BoundColumn();
			textCol.HeaderText = "&nbsp;";
			textCol.DataField = "RoleName";
			textCol.ItemStyle.Width = Unit.Parse("150px");
			dgRoleSelection.Columns.Add(textCol);
			var idCol = new BoundColumn();
			idCol.HeaderText = "";
			idCol.DataField = "roleid";
			idCol.Visible = false;
			dgRoleSelection.Columns.Add(idCol);
			TemplateColumn checkCol;
			checkCol = new TemplateColumn();
			var columnTemplate = new CheckBoxColumnTemplate();
			columnTemplate.DataField = "Selected";
			checkCol.ItemTemplate = columnTemplate;
			checkCol.HeaderText = Localization.GetString("SelectedRole");
			checkCol.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
			checkCol.HeaderStyle.Wrap = true;
			dgRoleSelection.Columns.Add(checkCol);
		}

		#endregion

		#region "Protected Methods"

		/// <summary>
		/// Load the ViewState
		/// </summary>
		/// <param name="savedState">The saved state</param>
		protected override void LoadViewState(object savedState)
		{
			if (savedState != null)
			{
				//Load State from the array of objects that was saved with SaveViewState.
				var myState = (object[]) savedState;
				
				//Load Base Controls ViewState
				if (myState[0] != null)
				{
					base.LoadViewState(myState[0]);
				}
				
				//Load TabPermissions
				if (myState[1] != null)
				{
					string state = Convert.ToString(myState[1]);
					if (state != string.Empty)
					{
						CurrentRoleSelection = new ArrayList(state.Split(new[] {"##"}, StringSplitOptions.None));
					}
					else
					{
						CurrentRoleSelection = new ArrayList();
					}
				}
			}
		}

		/// <summary>
		/// Saves the ViewState
		/// </summary>
		protected override object SaveViewState()
		{
			var allStates = new object[2];

			//Save the Base Controls ViewState
			allStates[0] = base.SaveViewState();
			//Persist the TabPermisisons
			var sb = new StringBuilder();
			bool addDelimiter = false;
			foreach (string role in CurrentRoleSelection)
			{
				if (addDelimiter)
				{
					sb.Append("##");
				}
				else
				{
					addDelimiter = true;
				}
				sb.Append(role);
			}
			allStates[1] = sb.ToString();
			return allStates;
		}

		/// <summary>
		/// Creates the Child Controls
		/// </summary>
		protected override void CreateChildControls()
		{
			pnlRoleSlections = new Panel();
			pnlRoleSlections.CssClass = "dnnRolesGrid";

			//Optionally Add Role Group Filter
			PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
			ArrayList arrGroups = RoleController.GetRoleGroups(_portalSettings.PortalId);
			if (arrGroups.Count > 0)
			{
				lblGroups = new Label();
				lblGroups.Text = Localization.GetString("RoleGroupFilter");
				pnlRoleSlections.Controls.Add(lblGroups);

				pnlRoleSlections.Controls.Add(new LiteralControl("&nbsp;&nbsp;"));

				cboRoleGroups = new DropDownList();
				cboRoleGroups.AutoPostBack = true;

				cboRoleGroups.Items.Add(new ListItem(Localization.GetString("AllRoles"), "-2"));
				var liItem = new ListItem(Localization.GetString("GlobalRoles"), "-1");
				liItem.Selected = true;
				cboRoleGroups.Items.Add(liItem);
				foreach (RoleGroupInfo roleGroup in arrGroups)
				{
					cboRoleGroups.Items.Add(new ListItem(roleGroup.RoleGroupName, roleGroup.RoleGroupID.ToString()));
				}
				pnlRoleSlections.Controls.Add(cboRoleGroups);

				pnlRoleSlections.Controls.Add(new LiteralControl("<br/><br/>"));
			}
			dgRoleSelection = new DataGrid();
			dgRoleSelection.AutoGenerateColumns = false;
			dgRoleSelection.CellSpacing = 0;
			dgRoleSelection.GridLines = GridLines.None;
			dgRoleSelection.FooterStyle.CssClass = "dnnGridFooter";
			dgRoleSelection.HeaderStyle.CssClass = "dnnGridHeader";
			dgRoleSelection.ItemStyle.CssClass = "dnnGridItem";
			dgRoleSelection.AlternatingItemStyle.CssClass = "dnnGridAltItem";
			SetUpRolesGrid();
			pnlRoleSlections.Controls.Add(dgRoleSelection);

			Controls.Add(pnlRoleSlections);
		}

		/// <summary>
		/// Overrides the base OnPreRender method to Bind the Grid to the Permissions
		/// </summary>
		/// <history>
		///     [cnurse]    01/09/2006  Documented
		/// </history>
		protected override void OnPreRender(EventArgs e)
		{
			BindData();
		}

		/// <summary>
		/// Updates a Selection
		/// </summary>
		/// <param name="roleName">The name of the role</param>
		/// <param name="Selected"></param>
		protected virtual void UpdateSelection(string roleName, bool Selected)
		{
			var isMatch = false;
			foreach (string currentRoleName in CurrentRoleSelection)
			{
				if (currentRoleName == roleName)
				{
					//role is in collection
					if (!Selected)
					{
						//Remove from collection as we only keep selected roles
						CurrentRoleSelection.Remove(currentRoleName);
					}
					isMatch = true;
					break;
				}
			}
			
			//Rolename not found so add new
			if (!isMatch && Selected)
			{
				CurrentRoleSelection.Add(roleName);
			}
		}

		/// <summary>
		/// Updates the Selections
		/// </summary>
		protected void UpdateSelections()
		{
			EnsureChildControls();

			UpdateRoleSelections();
		}

		/// <summary>
		/// Updates the permissions
		/// </summary>
		protected void UpdateRoleSelections()
		{
			if (dgRoleSelection != null)
			{
				foreach (DataGridItem dgi in dgRoleSelection.Items)
				{
					int i = 2;
					if (dgi.Cells[i].Controls.Count > 0)
					{
						var cb = (CheckBox) dgi.Cells[i].Controls[0];
						UpdateSelection(dgi.Cells[0].Text, cb.Checked);
					}
				}
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// RoleGroupsSelectedIndexChanged runs when the Role Group is changed
		/// </summary>
		/// <history>
		///     [cnurse]    01/06/2006  Documented
		/// </history>
		private void RoleGroupsSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateSelections();
		}
		
		#endregion

	}
}
