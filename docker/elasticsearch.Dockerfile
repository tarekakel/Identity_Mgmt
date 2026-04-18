FROM docker.elastic.co/elasticsearch/elasticsearch:8.13.4

RUN bin/elasticsearch-plugin install --batch analysis-icu \
 && bin/elasticsearch-plugin install --batch analysis-phonetic
