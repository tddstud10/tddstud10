using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow.ViewModel
{
    public enum WorkspaceState
    {
        Uninitialized
        , Initialized
    }

    public class MainViewModel : ViewModelBase
    {
        private string _eventLog;

        public string EventLog
        {
            get { return _eventLog; }
            set
            {
                if (_eventLog == value)
                {
                    return;
                }

                _eventLog = value;
                RaisePropertyChanged(() => EventLog);
            }
        }

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

        private WorkspaceState _workspaceState;

        public WorkspaceState State
        {
            get { return _workspaceState; }
            set
            {
                if (_workspaceState == value)
                {
                    return;
                }

                _workspaceState = value;
                RaisePropertyChanged(() => State);
            }
        }

        public RelayCommand EnableDisableWorkspace { get; set; }

        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                Departments = new List<Department>()
                {
                    new Department("DepartmentX"),
                    new Department("DepartmentY")
                };

                EventLog = "This is the event log...";

                State = WorkspaceState.Uninitialized;
            }
            else
            {
                // Code runs "for real"
            }

            EnableDisableWorkspace = new RelayCommand(
                () =>
                {
                    if (State == WorkspaceState.Initialized)
                    {
                        State = WorkspaceState.Uninitialized;
                    }
                    else
                    {
                        State = WorkspaceState.Initialized;
                    }
                });
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