# Configure the Azure provider
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-terraform-state-eastus2"
    storage_account_name = "stterraformcirruslyiot"
    container_name       = "ccc-webapp"
    key                  = "development.tfstate"
  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.66.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.40.0"
    }

    azurecaf = {
      source  = "aztfmod/azurecaf"
      version = "2.0.0-preview3"
    }
  }


  required_version = ">= 1.1.0"
}

provider "azurerm" {
  features {}
}

provider "azuread" {
  tenant_id = local.b2cTenantId
}

# A Single RG that holds everything
resource "azurerm_resource_group" "this" {
  name     = azurecaf_name.primary-rg.result
  location = local.location

  tags = {
    app     = local.appName
    purpose = local.environment
  }
}




# Manually created. could import later if really need to manage
# resource "azurerm_aadb2c_directory" "example" {
#   country_code            = "US"
#   data_residency_location = "United States"
#   display_name            = "ccc-webapp-b2c-tenant"
#   domain_name             = "ccc-webapp.onmicrosoft.com"
#   resource_group_name     = azurerm_resource_group.this.name
#   sku_name                = "PremiumP1"
# }

