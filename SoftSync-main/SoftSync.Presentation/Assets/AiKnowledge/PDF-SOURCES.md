# AI PDF sources

The AI Assistant uses the files configured in `AiDocuments:Files` as local RAG sources.
PDF text is indexed in memory on the first AI question. Only the most relevant excerpts are sent with the learner question in one chat-completions request; the complete books are not sent on every request.

For deployment, mount the three PDFs on the server and override each path with configuration or environment variables:

- `AiDocuments__Files__0__Path`
- `AiDocuments__Files__1__Path`
- `AiDocuments__Files__2__Path`

Do not commit the source PDFs or API keys to Git.

## Hugging Face model

The default provider is Hugging Face Inference Providers using its OpenAI-compatible chat endpoint. Set the token through an environment variable:

`AiApi__ApiKey=hf_your_token`

The configured model is `Qwen/Qwen3-4B-Instruct-2507:cheapest`. You can override it with `AiApi__Model` without changing source code.
