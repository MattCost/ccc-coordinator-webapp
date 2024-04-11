# App Hosting
# Service plan for api and webapp

resource "azurerm_service_plan" "this" {
  name                = azurecaf_name.service_plan.result
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  os_type             = "Linux"
  sku_name            = "B1"

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
    always_on = true

    application_stack {
      dotnet_version = "7.0"
    }

  }

  # These become env vars, which can be read by the config provider (nested properties use : which becomes __ )
  app_settings = {
    AzureAdB2C__Instance     = "https://cccwebapp.b2clogin.com"
    AzureAdB2C__Domain       = "cccwebapp.onmicrosoft.com"
    AzureAdB2C__ClientId     = azuread_application.api.application_id
    AzureAdB2C__ClientSecret = azuread_application_password.api.value

    AzureAdB2C__SignUpSignInPolicyId = "B2C_1_SignupSignin"

    Swagger__B2CDomain    = "cccwebapp"
    Swagger__PolicyId     = "B2C_1_SignupSignin"
    Swagger__ClientId     = azuread_application.website.application_id
    Swagger__Scope        = "https://cccwebapp.onmicrosoft.com/ccc-webapp-api/API.Access"
    Swagger__ScopeDisplay = "access api as user" 


    STORAGE_ACT_CONNECTION_STRING = azurerm_storage_account.this.primary_connection_string
  }

  logs {
    detailed_error_messages = true
    failed_request_tracing  = true
    http_logs {
      file_system {
        retention_in_days = 4
        retention_in_mb   = 35
      }
    }
  }
}

resource "azurerm_linux_web_app" "website" {
  name                = azurecaf_name.website.result
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_service_plan.this.location
  service_plan_id     = azurerm_service_plan.this.id

  site_config {
    always_on = true

    application_stack {
      dotnet_version = "7.0"
    }
  }


  # These become env vars, which can be read by the config provider (nested properties use : which becomes __ )
  app_settings = {
    AzureAdB2C__Instance             = "https://cccwebapp.b2clogin.com"
    AzureAdB2C__Domain               = "cccwebapp.onmicrosoft.com"
    AzureAdB2C__ClientId             = azuread_application.website.application_id
    AzureAdB2C__ClientSecret         = azuread_application_password.website.value
    AzureAdB2C__SignUpSignInPolicyId = "B2C_1_SignupSignin"
    AzureAdB2C__SignOutCallbackPath  = "/signout/B2C_1_SignupSignin"

    API__CalledAPIScopes = "${local.api_uri}/${local.api_access_scope}"
    API__Scope           = "${local.api_uri}/${local.api_access_scope}"
    API__BaseUrl         = "https://${azurerm_linux_web_app.api.default_hostname}/api/"

  }

  logs {
    detailed_error_messages = true
    failed_request_tracing  = true
    http_logs {
      file_system {
        retention_in_days = 4
        retention_in_mb   = 35
      }
    }
  }

}
