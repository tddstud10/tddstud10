using System.Collections.Generic;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow.ViewModel
{
    public enum WorkspaceState
    {
        Unloaded,
        Loading,
        Loaded,
    }

    public class Workspace : ObservableObject
    {
        private List<Department> _departments;

        public List<Department> Departments
        {
            get { return _departments; }
            set
            {
                if (_departments == value)
                {
                    return;
                }

                _departments = value;
                RaisePropertyChanged(() => Departments);
            }
        }

        private WorkspaceState _state;

        public WorkspaceState State
        {
            get { return _state; }
            set
            {
                if (_state == value)
                {
                    return;
                }

                _state = value;
                RaisePropertyChanged(() => State);
            }
        }

        public Workspace()
        {
            _state = WorkspaceState.Unloaded;
            _departments = new List<Department>()
            {
                new Department("DepartmentX"),
                new Department("DepartmentY")
            };
        }

        public async Task Disable()
        {
            State = WorkspaceState.Unloaded;
            await Task.Delay(1000);
        }

        public async Task LoadOrUnload()
        {
            if (State == WorkspaceState.Unloaded)
            {
                State = WorkspaceState.Loading;
                await Task.Delay(2000);
                State = WorkspaceState.Loaded;
            }
            else
            {
                State = WorkspaceState.Unloaded;
                await Task.Delay(2000);
            }
        }

        public bool CanLoadOrUnload()
        {
            return State != WorkspaceState.Loading;
        }
    }

    public class Book : ViewModelBase
    {
        private string _bookName = string.Empty;

        public string BookName
        {
            get { return _bookName; }
            set
            {
                if (_bookName == value)
                {
                    return;
                }

                _bookName = value;
                RaisePropertyChanged(() => BookName);
            }
        }

        public Book(string bookname)
        {
            BookName = bookname;
        }
    }

    public class Department : ViewModelBase
    {
        private List<Course> _courses;

        public Department(string depname)
        {
            DepartmentName = depname;

            Courses = new List<Course>()
            {
                new Course("Course1"),
                new Course("Course2")
            };
        }

        public List<Course> Courses
        {
            get { return _courses; }

            set
            {
                if (_courses == value)
                {
                    return;
                }

                _courses = value;
                RaisePropertyChanged(() => Courses);
            }
        }

        public string DepartmentName { get; set; }
    }

    public class Course : ViewModelBase
    {
        private List<Book> _books;

        public Course(string coursename)
        {
            CourseName = coursename;
            Books = new List<Book>()
            {
                new Book("JJJJ"),
                new Book("KKKK"),
                new Book("OOOOO")
            };
        }

        public List<Book> Books
        {
            get { return _books; }
            set
            {
                if (_books == value)
                {
                    return;
                }

                _books = value;
                RaisePropertyChanged(() => Books);
            }
        }

        public string CourseName { get; set; }
    }
}
