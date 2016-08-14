using System;
using System.Collections.Generic;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class FeatureManager : ProvisioningManagerBase
    {
        public virtual Dictionary<string, FeatureAdderCreator> FeaturesToAdd { get; set; }
        public virtual Dictionary<string, FeatureRemoverCreator> FeaturesToRemove { get; set; }

        public virtual void ConfigureFeatures(ClientContext ctx)
        {
            ConfigureFeatures(ctx, ctx.Web);
        }

        public virtual void ConfigureFeatures(ClientContext ctx, Web web)
        {
            if (FeaturesToRemove != null)
            {
                foreach (var featureRemover in FeaturesToRemove.Values)
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose,
                        "Removing feature " +
                        (!string.IsNullOrEmpty(featureRemover.DisplayName)
                            ? featureRemover.DisplayName
                            : featureRemover.FeatureId.ToString()));
                    TryToRemove(ctx, web, featureRemover);
                }
            }
            if (FeaturesToAdd != null)
            {
                foreach (var featureAdder in FeaturesToAdd.Values)
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose,
                        "Adding feature " +
                        (!string.IsNullOrEmpty(featureAdder.DisplayName)
                            ? featureAdder.DisplayName
                            : featureAdder.FeatureId.ToString()));
                    TryToAdd(ctx, web, featureAdder);
                }
            }
        }

        private void TryToAdd(ClientContext ctx, Web web, FeatureAdderCreator featureAdder)
        {
            try
            {
                if (featureAdder.FeatureDefinitionScope == FeatureDefinitionScope.Site)
                {
                    ctx.Site.Features.Add(featureAdder.FeatureId, featureAdder.Force, FeatureDefinitionScope.Farm);
                }
                else
                {
                    web.Features.Add(featureAdder.FeatureId, featureAdder.Force, FeatureDefinitionScope.Farm);
                }
                ctx.ExecuteQueryRetry();
            }
            catch (Exception ex)
            {
                OnNotify(ProvisioningNotificationLevels.Verbose,
                    "Error adding feature " +
                    (!string.IsNullOrEmpty(featureAdder.DisplayName)
                        ? featureAdder.DisplayName
                        : featureAdder.FeatureId.ToString()) + " | " + ex);
            }
        }

        private void TryToRemove(ClientContext ctx, Web web, FeatureRemoverCreator featureRemover)
        {
            try
            {
                if (featureRemover.FeatureDefinitionScope == FeatureDefinitionScope.Site)
                {
                    ctx.Site.Features.Remove(featureRemover.FeatureId, featureRemover.Force);
                }
                else
                {
                    web.Features.Remove(featureRemover.FeatureId, featureRemover.Force);
                }
                ctx.ExecuteQueryRetry();
            }
            catch
            {
                // ignored
            }
        }
    }
}