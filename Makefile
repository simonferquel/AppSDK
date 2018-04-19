docker-webstore:
	docker build -t docker-webstore .

deploy: docker-webstore
	kubectl apply -f deployment.yaml