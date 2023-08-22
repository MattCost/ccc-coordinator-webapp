will hold files to create infrastructure

TF run locally, using remote state.

Future setup.
Create SP, give SP contributor rights to the resource group, and blob storage read/write to the container.
Use this SP from pipelines.
This SP would need permission in a) main RG, b) B2C tenant (for app reg)
