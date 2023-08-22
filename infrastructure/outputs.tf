output "storage_account_name" {
  value = azurecaf_name.storage_account.result
}

output "app_service_plan_name" {
  value = azurecaf_name.service_plan.result
}

output "api_name" {
  value = azurecaf_name.api.result
}

output "website_name" {
  value = azurecaf_name.website.result
}