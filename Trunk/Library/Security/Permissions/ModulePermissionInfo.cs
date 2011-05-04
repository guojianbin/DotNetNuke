#region Copyright

// 
// DotNetNukeŽ - http://www.dotnetnuke.com
// Copyright (c) 2002-2011
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
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Security.Permissions
{
    [Serializable]
    public class ModulePermissionInfo : PermissionInfoBase, IHydratable
    {
        private int _moduleID;
        private int _modulePermissionID;

        public ModulePermissionInfo()
        {
            _modulePermissionID = Null.NullInteger;
            _moduleID = Null.NullInteger;
        }

        public ModulePermissionInfo(PermissionInfo permission) : this()
        {
            ModuleDefID = permission.ModuleDefID;
            PermissionCode = permission.PermissionCode;
            PermissionID = permission.PermissionID;
            PermissionKey = permission.PermissionKey;
            PermissionName = permission.PermissionName;
        }

        [XmlElement("modulepermissionid")]
        public int ModulePermissionID
        {
            get
            {
                return _modulePermissionID;
            }
            set
            {
                _modulePermissionID = value;
            }
        }

        [XmlElement("moduleid")]
        public int ModuleID
        {
            get
            {
                return _moduleID;
            }
            set
            {
                _moduleID = value;
            }
        }

        #region IHydratable Members

        public void Fill(IDataReader dr)
        {
            base.FillInternal(dr);
            ModulePermissionID = Null.SetNullInteger(dr["ModulePermissionID"]);
            ModuleID = Null.SetNullInteger(dr["ModuleID"]);
        }

        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return ModulePermissionID;
            }
            set
            {
                ModulePermissionID = value;
            }
        }

        #endregion

        public bool Equals(ModulePermissionInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return (AllowAccess == other.AllowAccess) && (ModuleID == other.ModuleID) && (RoleID == other.RoleID) && (PermissionID == other.PermissionID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof (ModulePermissionInfo))
            {
                return false;
            }
            return Equals((ModulePermissionInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_moduleID*397) ^ _modulePermissionID;
            }
        }
    }
}
