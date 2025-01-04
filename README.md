# EnvImpact Analysis
 Simple proof of concept project on the use of Durable Functions for two endpoints with single orchestration, Memory Cache, and OpenAI
* Two endpoints: http://localhost:xxx/api/species?name=(string)&region=(string)&percentage=(int?) and http://localhost:xxx/api/resources?name=(string)&region=(string)&percentage=(int?) to ChatGPT model.
* Query parameters: _name_ of animal, plants, etc., or _resources_ (specific mineral, natural resources), _region_ (of habitat or allocation), and _percentage_ of that reduction (of population or resource's presence). 
* Prompt questions are stored in respective files. Prompts are submitted to ChatGPT, and responses are stored in local files. 
* Durable functions are able to accommodate to any delays in processing requests.

* additional endpoint http://localhost:xxx/api/images?name=(string)&additional=(string)&type=(int?) to DALL-e-2 model
* Query parameters: _name_ - main object, _additional_ - any additional info for prompt, will come with the word "with" after the _name_, and _type_ -  enumerable value based on: 1 - Photography, 2 - Drawing, 3 - Painting and 4 - Sketch.
* Function creates or updates file with url to the generated image
