﻿using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

namespace CfeParametrizacao_0008
{
     //------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

    using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.8.3928.0.
// 


    /// <remarks/>
    [GeneratedCode("xsd", "4.8.3928.0")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class consParam
    {

        private string tpAmbField;

        private string cUFField;

        private string nSegField;

        private string dhEnvioField;

        private string nserieSATField;

        private string versaoField;

        private XmlAttribute[] anyAttrField;

        /// <remarks/>
        public string tpAmb
        {
            get { return this.tpAmbField; }
            set { this.tpAmbField = value; }
        }

        /// <remarks/>
        public string cUF
        {
            get { return this.cUFField; }
            set { this.cUFField = value; }
        }

        /// <remarks/>
        public string nSeg
        {
            get { return this.nSegField; }
            set { this.nSegField = value; }
        }

        /// <remarks/>
        public string dhEnvio
        {
            get { return this.dhEnvioField; }
            set { this.dhEnvioField = value; }
        }

        /// <remarks/>
        public string nserieSAT
        {
            get { return this.nserieSATField; }
            set { this.nserieSATField = value; }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string versao
        {
            get { return this.versaoField; }
            set { this.versaoField = value; }
        }

        /// <remarks/>
        [XmlAnyAttribute()]
        public XmlAttribute[] AnyAttr
        {
            get { return this.anyAttrField; }
            set { this.anyAttrField = value; }
        }
    }

}