set -e
docker build . --target=lighthouse-runtime -t aleskov/mightycalc-lighthouse:stable
docker build . --target=api-runtime -t aleskov/mightycalc-api:stable
docker build . --target=node-runtime  -t aleskov/mightycalc-node:stable
docker build . --target=build-env -t aleskov/mightycalc-test:stable
