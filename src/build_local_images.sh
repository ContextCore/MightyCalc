docker build . -t aleskov/mightycalc-lighthouse:stable --target=lighthouse-runtime
docker build . -t aleskov/mightycalc-node:stable --target=node-runtime
docker build . -t aleskov/mightycalc-api:stable --target=api-runtime
