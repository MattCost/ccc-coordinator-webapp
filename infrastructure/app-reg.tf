data "azuread_application_published_app_ids" "well_known" {}

data "azuread_client_config" "current" {}


resource "random_uuid" "apiaccess_scope_id" {}
resource "random_uuid" "apiadmin_scope_id" {}



resource "azuread_application" "api" {
  display_name = "CCC Webapp API"
  # needed? portal created apps dont have this
  identifier_uris  = ["https://cccwebapp.onmicrosoft.com/ccc-webapp-api"]
  owners           = [data.azuread_client_config.current.object_id]
  sign_in_audience = "AzureADandPersonalMicrosoftAccount"

  api {
    # mapped_claims_enabled          = true     # what does this do?
    requested_access_token_version = 2
    oauth2_permission_scope {
      admin_consent_description  = "Allows the app to access the api on behalf of a user."
      admin_consent_display_name = "Access API"
      user_consent_description   = "Allows you to interact with the application"
      user_consent_display_name  = "Access Application"
      enabled                    = true
      id                         = random_uuid.apiaccess_scope_id.result
      type                       = "User"
      value                      = "API.Access"
    }

    oauth2_permission_scope {
      admin_consent_description  = "Administer the application"
      admin_consent_display_name = "Administer"
      enabled                    = true
      id                         = random_uuid.apiadmin_scope_id.result
      type                       = "Admin"
      value                      = "API.Admin"
    }
  }

  web {
    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
    redirect_uris = ["http://localhost:8080/", "https://jwt.ms/"]
  }

  required_resource_access {
    resource_app_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph

    # Terraform datasource of well_known IDs doesn&#39;t contain openid and offline_access
    resource_access {
      id   = "37f7f235-527c-4136-accd-4a02d197296e" # openid
      type = "Scope"
    }

    resource_access {
      id   = "7427e0e9-2fba-42fe-b0c0-848c9e6a8182" # offline_access
      type = "Scope"
    }
  }
}

resource "azuread_service_principal" "api" {
  application_id = azuread_application.api.application_id
}


resource "azuread_application" "website" {
  display_name     = "CCC Webapp Website"
  identifier_uris  = ["https://cccwebapp.onmicrosoft.com/ccc-webapp-website"]
  owners           = [data.azuread_client_config.current.object_id]
  sign_in_audience = "AzureADandPersonalMicrosoftAccount"

  api {
    mapped_claims_enabled          = true     # what does this do?
    requested_access_token_version = 2
  }

  web {
    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
    redirect_uris = [ "https://jwt.ms/"]
  }

  required_resource_access {
    resource_app_id = azuread_application.api.application_id
    
    resource_access {
      id   = random_uuid.apiaccess_scope_id.result
      type = "Scope"
    }
  }
  required_resource_access {
    resource_app_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph

    resource_access {
      id   = "37f7f235-527c-4136-accd-4a02d197296e" # openid
      type = "Scope"
    }

    resource_access {
      id   = "7427e0e9-2fba-42fe-b0c0-848c9e6a8182" # offline_access
      type = "Scope"
    }

  }
}

resource "azuread_service_principal" "website" {
  application_id = azuread_application.website.application_id
}
