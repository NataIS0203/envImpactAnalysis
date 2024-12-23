# impactAnalysis
 Simple proof of concept project on the use of Durable Functions for two endpoints with single orchestration, Memory Cache, and OpenAI
* Two endpoints: http://localhost:xxx/api/species?name=(string)&region=(string)&percentage=(int?) and http://localhost:xxx/api/resources?name=(string)&region=(string)&percentage=(int?) .
* Query parameters: name of animal, plants, etc., or resources (specific mineral, natural resources), region (of habitat or allocation), and percentage of that reduction (of population or resource's presence). 
* Prompt questions are stored in respective files. Prompts are submitted to ChatGPT, and responses are stored in local files. 
* Durable functions are able to accommodate to any delays in processing requests.

