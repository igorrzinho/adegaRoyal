FROM quay.io/keycloak/keycloak:26.6.1

ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
CMD ["start-dev", "--http-port=8080", "--http-host=0.0.0.0"]