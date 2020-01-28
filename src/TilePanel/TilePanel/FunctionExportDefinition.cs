using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace TilePanel
{
    public class FunctionExportDefinition<T> : ExportDefinition
    {
        private FunctionExportDefinition(ExportDefinition definition, ComposablePartDefinition partDefinition)
            : base(definition.ContractName, definition.Metadata)
        {
            _mExportDefinition = definition;
            _mPartDefinition = partDefinition;
        }

        public static IEnumerable<FunctionExportDefinition<T>> GetExports(ComposablePartCatalog catalog)
        {
            return
                from part in catalog.Parts
                from definition in part.ExportDefinitions
                where definition.Metadata.ContainsKey(CompositionConstants.ExportTypeIdentityMetadataName)
                where typeof(T).FullName == definition.Metadata[CompositionConstants.ExportTypeIdentityMetadataName] as string
                select new FunctionExportDefinition<T>(definition, part);
        }

        public T GetValue()
        {
            return (T)_mPartDefinition.CreatePart().GetExportedValue(_mExportDefinition);
        }

        private readonly ExportDefinition _mExportDefinition;
        private readonly ComposablePartDefinition _mPartDefinition;
    }

    public static class CompositionHelpers
    {
        public static IEnumerable<FunctionExportDefinition<T>> GetExports<T>(this ComposablePartCatalog catalog)
        {
            return FunctionExportDefinition<T>.GetExports(catalog);
        }
    }
}
