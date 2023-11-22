namespace EPAM.StudyGroups.Api.Models
{
    public class StudyGroup
    {
        public StudyGroup(int studyGroupId, string name, Subject subject, DateTime createDate, IEnumerable<User> users)
        {
            StudyGroupId = studyGroupId;
            Name = name;
            Subject = subject;
            CreateDate = createDate;
            Users = users;
        }

        //Some logic will be missing to validate values according to acceptance criteria,
        public int StudyGroupId { get; }
        
        public string Name { get; }
        
        public Subject Subject { get; }
        
        public DateTime CreateDate { get; }
        
        public List<User> Users { get; private set; }
        
        public void AddUser(User user)
        {
            Users.Add(user);
        }

        public void RemoveUser(User user)
        {
            Users.Remove(user);
        }
    }
}