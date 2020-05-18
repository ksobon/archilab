using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using DynamoServices;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Wrapper for Schema utilities.
    /// </summary>
    [RegisterForTrace]
    public class Schema
    {
        internal Autodesk.Revit.DB.ExtensibleStorage.Schema InternalSchema
        {
            get;
            private set;
        }

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="schema">Autodesk Schema.</param>
        [SupressImportIntoVM]
        public Schema(Autodesk.Revit.DB.ExtensibleStorage.Schema schema)
        {
            InitSchema(schema);
        }

        private Schema(string guid, string name)
        {
            InitSchema(guid, name);
        }

        private void InitSchema(Autodesk.Revit.DB.ExtensibleStorage.Schema schema)
        {
            InternalSetSchema(schema);
        }

        private void InternalSetSchema(Autodesk.Revit.DB.ExtensibleStorage.Schema schema)
        {
            InternalSchema = schema;
        }

        private void InitSchema(string guid, string name)
        {
            Autodesk.Revit.DB.ExtensibleStorage.Schema schema;
            try
            {
                var sBuilder = new Autodesk.Revit.DB.ExtensibleStorage.SchemaBuilder(new Guid(guid));
                sBuilder.SetSchemaName(name);
                schema = sBuilder.Finish();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
            InternalSetSchema(schema);
        }

        /// <summary>
        /// Retrieves a Schema from current Document by its Guid.
        /// </summary>
        /// <param name="guid">Guid of the Schema.</param>
        /// <returns>Schema if found, otherwise null.</returns>
        public static Schema FindByGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(nameof(guid));
            }

            var existing = Autodesk.Revit.DB.ExtensibleStorage.Schema.Lookup(new Guid(guid));
            return existing != null ? new Schema(existing) : null;
        }

        /// <summary>
        /// Retrieves a Schema from current Document by its Name.
        /// </summary>
        /// <param name="name">Name of the Schema.</param>
        /// <returns>Schema if found, otherwise null.</returns>
        public static Schema FindByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var existing = Autodesk.Revit.DB.ExtensibleStorage.Schema.ListSchemas()
                .FirstOrDefault(x => x.SchemaName == name);
            return existing != null ? new Schema(existing) : null;
        }

        /// <summary>
        /// Retrieves all Schemas from the model.
        /// </summary>
        /// <returns>List of Schemas or empty collection.</returns>
        public static IEnumerable<Schema> GetAll()
        {
            return Autodesk.Revit.DB.ExtensibleStorage.Schema.ListSchemas().Select(x => new Schema(x));
        }

        /// <summary>
        /// Deletes Schema and all its Entities from Document.
        /// </summary>
        /// <param name="schema">Schema to delete.</param>
        /// <param name="overridePermissions">Normally Schema can only be edited by user with access permissions.
        /// Set this to true to override default access and force deletion of this Schema.</param>
        public static void EraseSchemaAndAllEntities(Schema schema, bool overridePermissions = false)
        {
            try
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                TransactionManager.Instance.EnsureInTransaction(doc);
                Autodesk.Revit.DB.ExtensibleStorage.Schema.EraseSchemaAndAllEntities(schema.InternalSchema, overridePermissions);
                TransactionManager.Instance.TransactionTaskDone();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Checks if current Addins has Write Access for this Schema.
        /// </summary>
        /// <param name="schema">Schema to check access for.</param>
        /// <returns>True if current Addin (Dynamo) can Write to this Schema, otherwise false.</returns>
        public static bool WriteAccessGranted(Schema schema)
        {
            return schema.InternalSchema.WriteAccessGranted();
        }

        /// <summary>
        /// Checks if current Addins has Read Access for this Schema.
        /// </summary>
        /// <param name="schema">Schema to check access for.</param>
        /// <returns>True if current Addin (Dynamo) can Read from this Schema, otherwise false.</returns>
        public static bool ReadAccessGranted(Schema schema)
        {
            return schema.InternalSchema.ReadAccessGranted();
        }

        /// <summary>
        /// Schema Name.
        /// </summary>
        public string Name
        {
            get { return InternalSchema.SchemaName; }
        }
        
        /// <summary>
        /// Schema Guid.
        /// </summary>
        public string Guid
        {
            get { return InternalSchema.GUID.ToString(); }
        }
        
        /// <summary>
        /// Schema Documentation/Description.
        /// </summary>
        public string Documentation
        {
            get { return InternalSchema.Documentation; }
        }

        /// <summary>
        /// Override for Schema display (Watch Node).
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Schema: Name:{InternalSchema.SchemaName}, Guid:{InternalSchema.GUID}";
        }
    }
}
