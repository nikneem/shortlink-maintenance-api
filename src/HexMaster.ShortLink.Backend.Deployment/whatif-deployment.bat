az group deployment create --resource-group shortlink-test-backend --template-file ./azuredeploy.json --parameters ./azuredeploy.test.parameters.json --mode Complete --what-if