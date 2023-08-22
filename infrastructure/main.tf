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

## Storage Act
# Storage
resource "azurerm_storage_account" "this" {
  name                     = azurecaf_name.storage_account.result
  resource_group_name      = azurerm_resource_group.this.name
  location                 = azurerm_resource_group.this.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = {
    app     = local.appName
    purpose = local.environment
  }
}

# App Hosting
# Service plan for api and webapp

resource "azurerm_service_plan" "this" {
  name                = azurecaf_name.service_plan.result
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  os_type             = "Linux"
  sku_name            = "F1"

  tags = {
    app     = local.appName
    purpose = local.environment
  }
}


resource "azurerm_linux_web_app" "api" {
  name                = azurecaf_name.api.result
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_service_plan.this.location
  service_plan_id     = azurerm_service_plan.this.id

  site_config {
    always_on = false
  }
}

resource "azurerm_linux_web_app" "website" {
  name                = azurecaf_name.website.result
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_service_plan.this.location
  service_plan_id     = azurerm_service_plan.this.id

  site_config {
    always_on = false
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

