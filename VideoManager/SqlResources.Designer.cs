﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace VideoManager {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SqlResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SqlResources() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("VideoManager.SqlResources", typeof(SqlResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性
        ///   重写当前线程的 CurrentUICulture 属性。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 CREATE TABLE dbo.Cat
        ///(
        ///    CatId INT IDENTITY(1,1) NOT NULL PRIMARY KEY, -- primary key column
        ///    CatName [NVARCHAR](50) NOT NULL,
        ///);
        /// 的本地化字符串。
        /// </summary>
        internal static string CreateCatTable {
            get {
                return ResourceManager.GetString("CreateCatTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 CREATE TABLE dbo.Tag
        ///(
        ///    TagId INT IDENTITY(1,1) NOT NULL PRIMARY KEY, -- primary key column
        ///    TagName [NVARCHAR](50) NOT NULL
        ///); 的本地化字符串。
        /// </summary>
        internal static string CreateTagTable {
            get {
                return ResourceManager.GetString("CreateTagTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 CREATE TABLE dbo.[user]
        ///(
        ///    UserId INT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- primary key column
        ///    Username [NVARCHAR](50) NOT NULL,
        ///    UserPassword [NVARCHAR](50) NOT NULL,
        ///    IsAdmin TINYINT NOT NULL
        ///); 的本地化字符串。
        /// </summary>
        internal static string CreateUserTable {
            get {
                return ResourceManager.GetString("CreateUserTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 CREATE TABLE dbo.Videos
        ///(
        ///    VideosId INT IDENTITY(1,1) NOT NULL PRIMARY KEY, -- primary key column
        ///    FilePath [NVARCHAR](MAX) NOT NULL,
        ///    VideoName [NVARCHAR](128) NOT NULL,
        ///    VideoTag INT NOT NULL,
        ///    VideoCat INT NOT NULL
        ///); 的本地化字符串。
        /// </summary>
        internal static string CreateVideosTale {
            get {
                return ResourceManager.GetString("CreateVideosTale", resourceCulture);
            }
        }
    }
}
