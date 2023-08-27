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

resource "azurerm_storage_table" "entities" {
  name                 = "Entities"
  storage_account_name = azurerm_storage_account.this.name
}
