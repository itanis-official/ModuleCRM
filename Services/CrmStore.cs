using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ModuleCRM.Models;

namespace ModuleCRM.Services
{
    /// <summary>
    /// In-memory store for CRM entities. This is a simple placeholder implementation for CRUD operations.
    /// Replace with a database-backed repository (EF Core, Dapper, etc.) when ready.
    /// </summary>
    public class CrmStore
    {
        private readonly object _lock = new();
        private int _companyId;
        private int _contactId;
        private int _projectId;
        private int _opportunityId;
        private int _userId;

        private readonly List<Company> _companies = new();
        private readonly List<Contact> _contacts = new();
        private readonly List<Project> _projects = new();
        private readonly List<Opportunity> _opportunities = new();
        private readonly List<User> _users = new();

        public IEnumerable<Company> Companies => _companies;
        public IEnumerable<Contact> Contacts => _contacts;
        public IEnumerable<Project> Projects => _projects;
        public IEnumerable<Opportunity> Opportunities => _opportunities;
        public IEnumerable<User> Users => _users;

        public Company AddCompany(Company company)
        {
            lock (_lock)
            {
                company.Id = ++_companyId;
                company.CreatedAt = DateTime.UtcNow;
                company.UpdatedAt = DateTime.UtcNow;
                _companies.Add(company);
                return company;
            }
        }

        public Company? GetCompany(int id)
        {
            lock (_lock)
            {
                return _companies.FirstOrDefault(c => c.Id == id);
            }
        }

        public void UpdateCompany(Company company)
        {
            lock (_lock)
            {
                var existing = _companies.FirstOrDefault(c => c.Id == company.Id);
                if (existing == null)
                    return;

                // Replace fields (references are kept for simplicity)
                existing.RaisonSociale = company.RaisonSociale;
                existing.MatriculeFiscal = company.MatriculeFiscal;
                existing.MatriculeFiscalCountry = company.MatriculeFiscalCountry;
                existing.Secteur = company.Secteur;
                existing.Logo = company.Logo;
                existing.Devis = company.Devis;
                existing.Adresse = company.Adresse;
                existing.CodePostal = company.CodePostal;
                existing.Ville = company.Ville;
                existing.Pays = company.Pays;
                existing.EmailPrincipal = company.EmailPrincipal;
                existing.EmailSecondaire = company.EmailSecondaire;
                existing.TelephonePrincipal = company.TelephonePrincipal;
                existing.TelephonePrincipalCountry = company.TelephonePrincipalCountry;
                existing.TelephoneSecondaire = company.TelephoneSecondaire;
                existing.TelephoneSecondaireCountry = company.TelephoneSecondaireCountry;
                existing.AgentResponsableId = company.AgentResponsableId;
                existing.Statut = company.Statut;
                existing.Notes = company.Notes;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        public bool DeleteCompany(int id)
        {
            lock (_lock)
            {
                var existing = _companies.FirstOrDefault(c => c.Id == id);
                if (existing == null)
                    return false;

                _companies.Remove(existing);
                return true;
            }
        }

        // Contacts
        public Contact AddContact(Contact contact)
        {
            lock (_lock)
            {
                contact.Id = ++_contactId;
                contact.CreatedAt = DateTime.UtcNow;
                contact.UpdatedAt = DateTime.UtcNow;
                _contacts.Add(contact);
                return contact;
            }
        }

        public Contact? GetContact(int id)
        {
            lock (_lock)
            {
                return _contacts.FirstOrDefault(c => c.Id == id);
            }
        }

        public void UpdateContact(Contact contact)
        {
            lock (_lock)
            {
                var existing = _contacts.FirstOrDefault(c => c.Id == contact.Id);
                if (existing == null)
                    return;

                existing.Nom = contact.Nom;
                existing.Prenom = contact.Prenom;
                existing.Poste = contact.Poste;
                existing.Email = contact.Email;
                existing.Telephone = contact.Telephone;
                existing.TelephoneCountry = contact.TelephoneCountry;
                existing.Login = contact.Login;
                existing.PasswordHash = contact.PasswordHash;
                existing.SendEmail = contact.SendEmail;
                existing.ForcePasswordChange = contact.ForcePasswordChange;
                existing.IsActive = contact.IsActive;
                existing.LastLogin = contact.LastLogin;
                existing.CompanyId = contact.CompanyId;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        public bool DeleteContact(int id)
        {
            lock (_lock)
            {
                var existing = _contacts.FirstOrDefault(c => c.Id == id);
                if (existing == null)
                    return false;
                _contacts.Remove(existing);
                return true;
            }
        }

        public IEnumerable<Contact> GetContactsByCompany(int companyId)
        {
            lock (_lock)
            {
                return _contacts.Where(c => c.CompanyId == companyId).ToList();
            }
        }

        // Projects
        public Project AddProject(Project project)
        {
            lock (_lock)
            {
                project.Id = ++_projectId;
                project.CreatedAt = DateTime.UtcNow;
                project.UpdatedAt = DateTime.UtcNow;
                _projects.Add(project);
                return project;
            }
        }

        public Project? GetProject(int id)
        {
            lock (_lock)
            {
                return _projects.FirstOrDefault(p => p.Id == id);
            }
        }

        public void UpdateProject(Project project)
        {
            lock (_lock)
            {
                var existing = _projects.FirstOrDefault(p => p.Id == project.Id);
                if (existing == null)
                    return;

                existing.Name = project.Name;
                existing.Reference = project.Reference;
                existing.Description = project.Description;
                existing.Status = project.Status;
                existing.StartDate = project.StartDate;
                existing.EndDate = project.EndDate;
                existing.CompanyId = project.CompanyId;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        public bool DeleteProject(int id)
        {
            lock (_lock)
            {
                var existing = _projects.FirstOrDefault(p => p.Id == id);
                if (existing == null)
                    return false;
                _projects.Remove(existing);
                return true;
            }
        }

        public IEnumerable<Project> GetProjectsByCompany(int companyId)
        {
            lock (_lock)
            {
                return _projects.Where(p => p.CompanyId == companyId).ToList();
            }
        }

        // Opportunities
        public Opportunity AddOpportunity(Opportunity opportunity)
        {
            lock (_lock)
            {
                opportunity.Id = ++_opportunityId;
                opportunity.CreatedAt = DateTime.UtcNow;
                opportunity.UpdatedAt = DateTime.UtcNow;
                _opportunities.Add(opportunity);
                return opportunity;
            }
        }

        public Opportunity? GetOpportunity(int id)
        {
            lock (_lock)
            {
                return _opportunities.FirstOrDefault(o => o.Id == id);
            }
        }

        public void UpdateOpportunity(Opportunity opportunity)
        {
            lock (_lock)
            {
                var existing = _opportunities.FirstOrDefault(o => o.Id == opportunity.Id);
                if (existing == null)
                    return;

                existing.Titre = opportunity.Titre;
                existing.Description = opportunity.Description;
                existing.ValeurEstimee = opportunity.ValeurEstimee;
                existing.Probabilite = opportunity.Probabilite;
                existing.PipelineStage = opportunity.PipelineStage;
                existing.DateCloturePrevu = opportunity.DateCloturePrevu;
                existing.DateCloture = opportunity.DateCloture;
                existing.Type = opportunity.Type;
                existing.SubType = opportunity.SubType;
                existing.AgentCommercialId = opportunity.AgentCommercialId;
                existing.AgentCdcId = opportunity.AgentCdcId;
                existing.EcheanceCdc = opportunity.EcheanceCdc;
                existing.CdcFilePath = opportunity.CdcFilePath;
                existing.Notes = opportunity.Notes;
                existing.CompanyId = opportunity.CompanyId;
                existing.ProjectParentId = opportunity.ProjectParentId;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        public bool DeleteOpportunity(int id)
        {
            lock (_lock)
            {
                var existing = _opportunities.FirstOrDefault(o => o.Id == id);
                if (existing == null)
                    return false;

                _opportunities.Remove(existing);
                return true;
            }
        }

        public IEnumerable<Opportunity> GetOpportunitiesByCompany(int companyId)
        {
            lock (_lock)
            {
                return _opportunities.Where(o => o.CompanyId == companyId).ToList();
            }
        }

        // Users
        public User AddUser(User user)
        {
            lock (_lock)
            {
                user.Id = ++_userId;
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                _users.Add(user);
                return user;
            }
        }

        public User? GetUser(int id)
        {
            lock (_lock)
            {
                return _users.FirstOrDefault(u => u.Id == id);
            }
        }

        public void UpdateUser(User user)
        {
            lock (_lock)
            {
                var existing = _users.FirstOrDefault(u => u.Id == user.Id);
                if (existing == null)
                    return;

                existing.Nom = user.Nom;
                existing.Prenom = user.Prenom;
                existing.Email = user.Email;
                existing.Telephone = user.Telephone;
                existing.Login = user.Login;
                existing.PasswordHash = user.PasswordHash;
                existing.Avatar = user.Avatar;
                existing.Role = user.Role;
                existing.IsActive = user.IsActive;
                existing.LastLogin = user.LastLogin;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        public bool DeleteUser(int id)
        {
            lock (_lock)
            {
                var existing = _users.FirstOrDefault(u => u.Id == id);
                if (existing == null)
                    return false;

                _users.Remove(existing);
                return true;
            }
        }
    }
}
