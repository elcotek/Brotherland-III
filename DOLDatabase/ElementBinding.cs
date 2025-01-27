﻿/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using DOL.Database.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace DOL.Database
{
    /// <summary>
    /// Data Element Binding
    /// </summary>
    public sealed class ElementBinding
    {
        /// <summary>
        /// Column Name of this Element Binding
        /// </summary>
        public string ColumnName { get; private set; }
        /// <summary>
        /// Value's Type of this Element Binding
        /// </summary>
        public Type ValueType { get; private set; }
        /// <summary>
        /// Get Value Object
        /// </summary>
        public Func<DataObject, object> GetValue { get; private set; }
        /// <summary>
        /// Set Value Object
        /// </summary>
        public Action<DataObject, object> SetValue { get; private set; }
        /// <summary>
        /// Get DataElement Attribute
        /// </summary>
        public DataElement DataElement { get; private set; }
        /// <summary>
        /// Get Relation Attribute
        /// </summary>
        public Relation Relation { get; private set; }
        /// <summary>
        /// Get Primary Key Attribute
        /// </summary>
        public PrimaryKey PrimaryKey { get; private set; }
        /// <summary>
        /// Get ReadOnly Attribute
        /// </summary>
        public ReadOnly ReadOnly { get; private set; }
        /// <summary>
        /// Check if this Element Binding Implement any Data Attribute
        /// </summary>
        public bool IsDataElementBinding { get { return DataElement != null || Relation != null || PrimaryKey != null; } }

        /// <summary>
        /// Create a new instance of <see cref="ElementBinding"/>
        /// </summary>
        /// <param name="Member">MemberInfo for this ElementBinding</param>
        public ElementBinding(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    var property = member as PropertyInfo;
                    ValueType = property.PropertyType;
                    GetValue = CreateGetter(property);
                    SetValue = CreateSetter(property);
                    break;
                case MemberTypes.Field:
                    var field = member as FieldInfo;
                    ValueType = field.FieldType;
                    GetValue = field.GetValue;
                    SetValue = field.SetValue;
                    break;
                default:
                    return;
            }

            ColumnName = member.Name;
            DataElement = GetCustomAttribute<DataElement>(member);
            Relation = GetCustomAttribute<Relation>(member);
            PrimaryKey = GetCustomAttribute<PrimaryKey>(member);
            ReadOnly = GetCustomAttribute<ReadOnly>(member);
        }

        // Beispiel für die Getter-Creation
        private Func<object, object> CreateGetter(PropertyInfo property)
        {
            return obj => property.GetValue(obj, null);
        }

        private Action<object, object> CreateSetter(PropertyInfo property)
        {
            return (obj, val) => property.SetValue(obj, val, null);
        }

        // Allgemeine Methode zum Caching von Attributen
        private T GetCustomAttribute<T>(MemberInfo member) where T : Attribute
        {
            return member.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        }


        /// <summary>
        /// Create a custom instance of <see cref="ElementBinding"/>
        /// </summary>
        /// <param name="Member">MemberInfo for this ElementBinding</param>
        /// <param name="DataElement">Custom DataElement Attached</param>
        /// <param name="ColumnName">Custom ColumnName</param>
        public ElementBinding(MemberInfo Member, DataElement DataElement, string ColumnName)
            : this(Member)
        {
            this.DataElement = DataElement;
            this.ColumnName = ColumnName;
        }

        /// <summary>
        /// Create a custom instance of <see cref="ElementBinding"/> with Primary Key
        /// </summary>
        /// <param name="Member">MemberInfo for this ElementBinding</param>
        /// <param name="PrimaryKey">Custom PrimaryKey Attached</param>
        /// <param name="ColumnName">Custom ColumnName</param>
        public ElementBinding(MemberInfo Member, PrimaryKey PrimaryKey, string ColumnName)
            : this(Member)
        {
            this.PrimaryKey = PrimaryKey;
            this.ColumnName = ColumnName;
        }
    }
}
