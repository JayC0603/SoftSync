# SoftSync AI Integration Notes

This document describes how to integrate the real AI services provided by the AI Team (Kiệt & Chánh).

## Architecture

The application uses a 3-layer architecture. AI interfaces are defined in the **Business Logic Layer (BLL)**:

- Projects: `SoftSync.BLL`
- Namespace: `SoftSync.BLL.Interfaces`

## Interfaces to Implement

### 1. IAiAssessmentService

- **Method**: `Task<AssessmentResultDto> EvaluateAsync(List<UserAnswerDto> answers)`
- **Role**: Evaluates the user's questionnaire answers and provides scores/levels for specific soft skills.
- **Current Fake Implementation**: `SoftSync.BLL.Services.Fake.FakeAiAssessmentService`

### 2. IAiAssistantService

- **Method**: `Task<string> GetReplyAsync(string userMessage, int userId)`
- **Role**: Provides chatbot responses to help students with learning soft skills.
- **Current Fake Implementation**: `SoftSync.BLL.Services.Fake.FakeAiAssistantService`

### 3. IAiRoadmapService

- **Method**: `Task<RoadmapDto> GenerateRoadmapAsync(int userId, List<string> weakSkills)`
- **Role**: Generates a personalized learning roadmap based on identified weaknesses.
- **Current Fake Implementation**: `SoftSync.BLL.Services.Fake.FakeAiRoadmapService`

## Integration Steps for AI Team

1. **Create New Service**: In `SoftSync.BLL.Services`, create a new class implementing the respective interface.
2. **Use HttpClient**: Use the named `HttpClient` ("AiApi") registered in `Program.cs`. Example:
   ```csharp
   public class AiAssessmentService : IAiAssessmentService {
       private readonly HttpClient _httpClient;
       public AiAssessmentService(IHttpClientFactory factory) {
           _httpClient = factory.CreateClient("AiApi");
       }
       // implementation...
   }
   ```
3. **Update Dependency Injection**: In `SoftSync.Presentation/Program.cs`, replace the Fake registration with your new implementation:
   ```csharp
   // TODO [AI-TEAM: Kiệt/Chánh] - Replace Fake service with real one
   // builder.Services.AddScoped<IAiAssessmentService, FakeAiAssessmentService>();
   builder.Services.AddScoped<IAiAssessmentService, AiAssessmentService>();
   ```
4. **Configuration**: Set the base URL in `appsettings.json` under `AiApi:BaseUrl`.
