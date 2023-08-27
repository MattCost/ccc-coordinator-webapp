# Overview

Terraform files for this webapp.

# File Layout
- main.tf - main entrypoint
- locals.tf - customize settings here
- outputs.tf - outputs generated here
- naming.tf - naming convetion blocks here
- *.tf - the rest of the filenames describe what role they play.
- app-reg.tf - azure ad app registrations
- app-service.tf - the azuread app service plan and apps
- storage.tf - the azure storage act, and tables.

# Running

Setup to use remote state. VSCode has tasks for plan and apply.

Future setup.
Create SP, give SP contributor rights to the resource group, and blob storage read/write to the container.
Use this SP from pipelines.
This SP would need permission in a) main RG, b) B2C tenant (for app reg)

TODO

turn off implicit flow for website login.
    make seperate app reg with implicit flow for swagger.
    (does this matter?)

Create SP. give this SP rights to the RG
    use this SP for TF.

Create SP. give this SP rights to deploy webapps
    use this SP as an azure service connection for devops pipelines to build and deploy the code