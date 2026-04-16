VERSION ?= 1.0


push-image:
	bash cloud/deploy/image/main.sh --version $(VERSION)

deploy-app:
	bash cloud/deploy/app/main.sh

destroy-app:
	bash cloud/destroy/app/main.sh

show-plan:
	cd ../IAC-Terraform && terraform init && terraform plan

local-deploy:
	bash cloud/deploy/main.sh

local-destroy:
	bash local/destroy/main.sh
	