docker-webstore:
	docker build -t docker-webstore .

deploy: docker-webstore
	kubectl apply -f deployment.yaml

restart: deploy
	kubectl scale --replicas=0 statefulsets/docker-webstore
	kubectl scale --replicas=1 statefulsets/docker-webstore