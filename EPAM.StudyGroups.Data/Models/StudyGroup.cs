namespace EPAM.StudyGroups.Data.Models
{
    public class StudyGroup
    {
        public StudyGroup()
        {
        }

        public StudyGroup(int studyGroupId, string name, Subject subject, DateTime createDate, IEnumerable<User> users)
        {
            StudyGroupId = studyGroupId;
            Name = name;
            Subject = subject;
            CreateDate = createDate;
            Users = new List<User>(users.Count());
            Users.AddRange(users);
        }

        //Some logic will be missing to validate values according to acceptance criteria,
        public int? StudyGroupId { get; set; }

        public string Name { get; set; }

        public Subject Subject { get; set; }

        public DateTime CreateDate { get; set; }

        public List<User> Users { get; private set; }

        public void AddUser(User user)
        {
            if (Users == null)
            {
                Users = new List<User>();
            }

            Users.Add(user);
        }

        public void RemoveUser(User user)
        {
            Users.Remove(user);
        }
    }
}