# Données de test pour Swagger (ModuleCRM)

Ce document fournit des exemples de corps de requête JSON à utiliser avec l'interface Swagger (OpenAPI) exposée par le backend **ModuleCRM**. Il est conçu pour faciliter les tests en utilisant "Try it out" dans Swagger UI.

> ⚠️ Note : certains champs (comme `CreatedAt`, `UpdatedAt`, `Id`) sont générés automatiquement par le backend et ne sont pas forcément nécessaires dans les requêtes de création.

---

## Base URL

Par défaut, l'API est exposée sur :

```
https://localhost:<port>/api
```

Remplacez `<port>` par le port utilisé par votre application (ex : `7121`, `5001`, etc.).

---

## 1) Companies (Sociétés)

### Créer une société (POST /api/Companies)

```json
{
  "RaisonSociale": "Acme Corp",
  "MatriculeFiscal": "12345678",
  "MatriculeFiscalCountry": "TN",
  "Secteur": "Technologie",
  "Logo": "https://example.com/logo.png",
  "Devis": "Devis 001",
  "Adresse": "1 Rue de la Paix",
  "CodePostal": "1000",
  "Ville": "Tunis",
  "Pays": "Tunisie",
  "EmailPrincipal": "contact@acme.tn",
  "EmailSecondaire": "support@acme.tn",
  "TelephonePrincipal": "12345678",
  "TelephonePrincipalCountry": "+216",
  "TelephoneSecondaire": "87654321",
  "TelephoneSecondaireCountry": "+216",
  "AgentResponsableId": 1,
  "Statut": "prospect",
  "Notes": "Société cliente potentielle.",
  "CreatedAt": "2026-03-16T00:00:00Z",
  "UpdatedAt": "2026-03-16T00:00:00Z"
}
```

### Mettre à jour une société (PUT /api/Companies/{id})

```json
{
  "Id": 1,
  "RaisonSociale": "Acme Corp - Mise à jour",
  "MatriculeFiscal": "12345678",
  "MatriculeFiscalCountry": "TN",
  "Secteur": "Technologie",
  "Logo": "https://example.com/logo-updated.png",
  "Devis": "Devis 002",
  "Adresse": "2 Avenue des Entrepreneurs",
  "CodePostal": "1001",
  "Ville": "Tunis",
  "Pays": "Tunisie",
  "EmailPrincipal": "contact@acme.tn",
  "EmailSecondaire": "support@acme.tn",
  "TelephonePrincipal": "12345678",
  "TelephonePrincipalCountry": "+216",
  "TelephoneSecondaire": "87654321",
  "TelephoneSecondaireCountry": "+216",
  "AgentResponsableId": 1,
  "Statut": "client",
  "Notes": "Client actif.",
  "CreatedAt": "2026-03-16T00:00:00Z",
  "UpdatedAt": "2026-03-16T00:00:00Z"
}
```

---

## 2) Contacts

### Créer un contact (POST /api/Contacts)

```json
{
  "CompanyId": 1,
  "Nom": "Ben",
  "Prenom": "Youssef",
  "Poste": "Responsable commercial",
  "Email": "youssef.ben@acme.tn",
  "Telephone": "98765432",
  "TelephoneCountry": "+216",
  "Login": "yben",
  "PasswordHash": "hashed-password-example",
  "SendEmail": true,
  "ForcePasswordChange": true,
  "IsActive": true,
  "LastLogin": "2026-03-16T00:00:00Z",
  "CreatedAt": "2026-03-16T00:00:00Z",
  "UpdatedAt": "2026-03-16T00:00:00Z"
}
```

### Mettre à jour un contact (PUT /api/Contacts/{id})

```json
{
  "Id": 1,
  "CompanyId": 1,
  "Nom": "Ben",
  "Prenom": "Youssef",
  "Poste": "Directeur commercial",
  "Email": "youssef.ben@acme.tn",
  "Telephone": "98765432",
  "TelephoneCountry": "+216",
  "Login": "yben",
  "PasswordHash": "hashed-password-example",
  "SendEmail": true,
  "ForcePasswordChange": false,
  "IsActive": true,
  "LastLogin": "2026-03-16T00:00:00Z",
  "CreatedAt": "2026-03-16T00:00:00Z",
  "UpdatedAt": "2026-03-16T00:00:00Z"
}
```

---

## 3) Projets (Projects)

### Créer un projet (POST /api/Projects)

```json
{
  "CompanyId": 1,
  "Name": "Projet Alpha",
  "Reference": "ALPHA-2026",
  "Description": "Migration de la plateforme vers une nouvelle version.",
  "Status": "actif",
  "StartDate": "2026-04-01T00:00:00Z",
  "EndDate": "2026-09-30T00:00:00Z",
  "CreatedAt": "2026-03-16T00:00:00Z",
  "UpdatedAt": "2026-03-16T00:00:00Z"
}
```

### Mettre à jour un projet (PUT /api/Projects/{id})

```json
{
  "Id": 1,
  "CompanyId": 1,
  "Name": "Projet Alpha - Phase 2",
  "Reference": "ALPHA-2026",
  "Description": "Extension du projet pour intégrer une nouvelle API.",
  "Status": "actif",
  "StartDate": "2026-04-01T00:00:00Z",
  "EndDate": "2026-12-31T00:00:00Z",
  "CreatedAt": "2026-03-16T00:00:00Z",
  "UpdatedAt": "2026-03-16T00:00:00Z"
}
```

---

## 4) Opportunités

### Créer une opportunité (POST /api/Opportunities)

```json
{
  "CompanyId": 1,
  "ProjectParentId": 1,
  "Titre": "Offre de maintenance annuelle",
  "Description": "Renouvellement du contrat de maintenance.",
  "ValeurEstimee": 15000.00,
  "Probabilite": 70,
  "PipelineStage": "negociation",
  "DateCloturePrevu": "2026-06-15T00:00:00Z",
  "DateCloture": null,
  "Type": "renouvellement",
  "SubType": "contrat",
  "AgentCommercialId": 1,
  "AgentCdcId": 1,
  "EcheanceCdc": "2026-05-01T00:00:00Z",
  "CdcFilePath": "https://example.com/cdc.pdf",
  "Notes": "Prioritaire",
  "CreatedAt": "2026-03-16T00:00:00Z",
  "UpdatedAt": "2026-03-16T00:00:00Z"
}
```

### Mettre à jour une opportunité (PUT /api/Opportunities/{id})

```json
{
  "Id": 1,
  "CompanyId": 1,
  "ProjectParentId": 1,
  "Titre": "Offre de maintenance annuelle - ajustée",
  "Description": "Renouvellement du contrat de maintenance avec un ajustement tarifaire.",
  "ValeurEstimee": 16000.00,
  "Probabilite": 80,
  "PipelineStage": "gagné",
  "DateCloturePrevu": "2026-06-15T00:00:00Z",
  "DateCloture": "2026-06-10T00:00:00Z",
  "Type": "renouvellement",
  "SubType": "contrat",
  "AgentCommercialId": 1,
  "AgentCdcId": 1,
  "EcheanceCdc": "2026-05-01T00:00:00Z",
  "CdcFilePath": "https://example.com/cdc-updated.pdf",
  "Notes": "Opportunité gagnée.",
  "CreatedAt": "2026-03-16T00:00:00Z",
  "UpdatedAt": "2026-03-16T00:00:00Z"
}
```

---

## 5) Utilisateurs (Users)

### Créer un utilisateur (POST /api/Users)

```json
{
  "Nom": "Zitouni",
  "Prenom": "Sami",
  "Email": "sami.zitouni@acme.tn",
  "Telephone": "+21612345678",
  "Login": "szitouni",
  "PasswordHash": "hashed-password-example",
  "Avatar": "https://example.com/avatar.png",
  "Role": "admin",
  "IsActive": true,
  "LastLogin": "2026-03-16T00:00:00Z",
  "CreatedAt": "2026-03-16T00:00:00Z",
  "UpdatedAt": "2026-03-16T00:00:00Z"
}
```

### Mettre à jour un utilisateur (PUT /api/Users/{id})

```json
{
  "Id": 1,
  "Nom": "Zitouni",
  "Prenom": "Sami",
  "Email": "sami.zitouni@acme.tn",
  "Telephone": "+21612345678",
  "Login": "szitouni",
  "PasswordHash": "hashed-password-example",
  "Avatar": "https://example.com/avatar.png",
  "Role": "admin",
  "IsActive": true,
  "LastLogin": "2026-03-16T00:00:00Z",
  "CreatedAt": "2026-03-16T00:00:00Z",
  "UpdatedAt": "2026-03-16T00:00:00Z"
}
```

---

## Conseils pour les tests Swagger

- Dans l'interface Swagger, utilisez `Try it out` pour envoyer des requêtes.
- Assurez-vous de 
  - **envoyer des objets JSON valides**,
  - **inclure les champs requis** (`[Required]`) comme indiqué plus haut,
  - **choisir des `Id` de référence existants** (par exemple, `CompanyId`, `AgentCommercialId`, etc.).
- Si vous testez des requêtes PUT ou DELETE, récupérez d'abord les données via `GET` pour connaître les `Id` disponibles.

---

Bonne exploration !
