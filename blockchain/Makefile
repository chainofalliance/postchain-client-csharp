PROJECT=csharp_test
export CHR_CLI_VERSION := 0.10.0

.PHONY: start
start:
	docker compose -p ${PROJECT} up blockchain -d

.PHONY: restart
restart: stop clear start

.PHONY: clear
clear:
	docker compose -p ${PROJECT} down -v

.PHONY: stop
stop:
	docker compose -p ${PROJECT} down
