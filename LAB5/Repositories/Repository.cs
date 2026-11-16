using System;
using System.Collections.Generic;
using System.Linq;
namespace LAB5.Repositories{
    public class Repository<T>{
        private readonly List<T> _items = new();
        public void Add(T item){
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _items.Add(item);
        }
        public bool Remove(T item) {
            return _items.Remove(item);
        }
        public IEnumerable<T> Find(Func<T, bool> predicate) {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _items.Where(predicate);
        }
        public List<T> GetAll() {
            return new List<T>(_items);
        }
        public void Clear(){
            _items.Clear();
        }
    }
}
