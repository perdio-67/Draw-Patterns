using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Draw
{
    public class shape_class
    {
        public int id;
        public string path;
        public string short_path;
        private Boolean _visible;
        private Boolean _touchable;
        public Boolean _pattern;
        public TouchControl touch;
        public Bitmap original_bitmap;
        public Bitmap bitmap;
        public System.Drawing.Color original_color;
        public System.Drawing.Color current_color;

        public shape_class(int id, string path, string short_path, bool visible, bool touchable, bool pattern, TouchControl touch, System.Drawing.Color current_color)
        {
            this.id = id;
            this.path = path;
            this.short_path = short_path;
            _visible = visible;
            _touchable = touchable;
            this.touch = touch;
            this._pattern = pattern;
            this.original_color = current_color;
            this.current_color = current_color;
            this.bitmap = new Bitmap(path);
            this.original_bitmap = new Bitmap(path); ;
        }

        public void set_visible(Boolean visible)
        {
            _visible = visible;

            if(_visible)
            {
                this.touch.show();
            }
            else
                this.touch.hide();
        }

        //public Bitmap b;

    }
}
