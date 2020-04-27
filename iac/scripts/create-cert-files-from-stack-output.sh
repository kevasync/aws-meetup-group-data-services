OUTPUT_DIR="../iot-producer/secure"
# rm -rf "${OUTPUT_DIR}"
# mkdir "${OUTPUT_DIR}"
# pulumi stack output CertPublicKey > "${OUTPUT_DIR}/sensor-producer-thing.public"
# pulumi stack output CertPrivateKey > "${OUTPUT_DIR}/sensor-producer-thing.private"
# pulumi stack output CertPem > "${OUTPUT_DIR}/sensor-producer-thing.pem"
# curl -o "${OUTPUT_DIR}/amzn.crt" "https://www.amazontrust.com/repository/AmazonRootCA1.pem"

openssl pkcs12 -export -in "${OUTPUT_DIR}/db6db19cdc-certificate.pem.crt" \
 -inkey "${OUTPUT_DIR}/db6db19cdc-private.pem.key" \
 -out "${OUTPUT_DIR}/db6db19cdc.pfx" \
 -certfile "${OUTPUT_DIR}/amzn.crt" \
 -password "pass:MyPassword1"
