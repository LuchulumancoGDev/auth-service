# auth-service
This is an ASP.NET Core authentication API that handles user registration, login, and token-based authentication using jwt and refresh tokens

## branching-strategy
- main->Production
- staging -> Testing/QA
- dev -> Development
- feature/* -> New features

## workflow
- Features are developed in feature/* branches
- Merged into dev via pull requests
- Promoted from dev -> staging -> main

## Rules
- No direct pushes to dev, staging or main
- All changes go through pull requests
- Code reviews and checks are required

## CI/CD
Automated using Github ACtions:
- build and test on every push/PR
- Deploy per environment:
- dev -> Dev
- staging -> Staging
- main -> Production

