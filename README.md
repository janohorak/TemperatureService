TemperatureService

Ukážkový ASP.NET Core projekt zameraný na moderný backend development a infraštruktúru.

Projekt obsahuje:
- REST API
- Redis cache
- background refresh service
- Bearer token autentifikáciu
- Docker a Docker Compose
- Kubernetes deployment
- Ingress routing

Architektúra

Klient
  ↓
Temperature API
  ↓
Redis Cache




Kubernetes verzia:
Browser
  ↓
Ingress
  ↓
Service
  ↓
Temperature API Pod
  ↓
Redis Service
  ↓
Redis Pod

Použité technológie
- ASP.NET Core
- Redis
- Swagger
- Docker
- Docker Compose
- Kubernetes
- HttpClientFactory
- BackgroundService

Build Docker image
docker build -t temperatureservice-api:latest -f src/TemperatureService.Api/Dockerfile .

Deploy do Kubernetes
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/ingress.yaml

Overenie
Pody:
kubectl get pods

Services:
kubectl get services

Ingress:
kubectl get ingress

Zmena počtu API inštancií:
kubectl scale deployment temperature-api --replicas=3

Logy:
kubectl logs deployment/temperature-api

Reštart deploymentu:
kubectl rollout restart deployment/temperature-api

Port forwarding:
kubectl port-forward service/temperature-api 8080:80