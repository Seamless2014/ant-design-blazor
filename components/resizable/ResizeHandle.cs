using System;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace AntDesign
{
    public class ResizeHandle : AntDomComponentBase, IDisposable
    {
        [Parameter]
        public int Index { get; set; }

        [Parameter]
        public bool VerticalOrHorizontal { get; set; } = false;

        [Parameter]
        public bool IsDiagonal { get; set; } = false;

        [Parameter]
        public double Width { get; set; } = 5;

        [Parameter]
        public double Height { get; set; } = 30;

        [Parameter]
        public string BgColor { get; set; } = "silver";

        [Parameter]
        public Action<bool, int, int> OnPositionChange { get; set; }

        [Parameter]
        public Action<int, int, int> OnDiagonalPositionChange { get; set; }

        [Parameter]
        public Action<int, int, int> OnDragStart { get; set; }

        [Parameter]
        public Action<int, int, int> OnDragEnd { get; set; }

        private readonly Splitter _splitter = new Splitter();

        private bool _dragMode = false;

        [Parameter]
        public bool EnableRender { get; set; } = true;

        protected override void OnInitialized()
        {
            _dragMode = false;

            base.OnInitialized();
        }

        protected override bool ShouldRender()
        {
            return EnableRender;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (EnableRender && builder != null)
            {
                base.BuildRenderTree(builder);

                int i = 0;
                builder.OpenElement(i++, "div");
                builder.AddAttribute(i++, "id", Id);
                builder.AddAttribute(i++, "style", GetStyle());
                builder.AddAttribute(i++, "onpointerdown", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerDown));
                builder.AddAttribute(i++, "onpointermove", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerMove));
                builder.AddAttribute(i++, "onpointerup", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerUp));

                builder.AddEventPreventDefaultAttribute(i++, "onmousemove", true);

                builder.AddElementReferenceCapture(i++, (value) => { Ref = value; });
                //builder.AddAttribute(k++, "onmousemove", EventCallback.Factory.Create<MouseEventArgs>(this, "return false;")); //event.preventDefault()

                builder.CloseElement();

                EnableRender = false;
            }
        }

        internal string GetStyle()
        {
            StringBuilder sb1 = new StringBuilder();

            sb1.Append("width:" + Width + "px;height:" + Height + "px;");

            if (!VerticalOrHorizontal)
            {
                sb1.Append("display:inline-block;");
            }

            //sb1.Append("background-color:red;");
            sb1.Append("background-color:" + BgColor + ";");

            if (IsDiagonal)
            {
                sb1.Append("cursor:nwse-resize;");
            }
            else
            {
                if (VerticalOrHorizontal)
                {
                    sb1.Append("cursor:s-resize;");
                    //sb1.Append("cursor:col-resize;");
                }
                else
                {
                    sb1.Append("cursor:w-resize;");
                    //sb1.Append("cursor:col-resize;");
                }
            }

            return sb1.ToString();
        }

        private void OnPointerDown(PointerEventArgs e)
        {
            JsInvokeAsync<bool>(JSInteropConstants.SetPointerCapture, Ref, e.PointerId);
            _dragMode = true;

            if (IsDiagonal)
            {
                _splitter.PreviousPosition = (int)e.ClientX;
                _splitter.PreviousPosition2 = (int)e.ClientY;
            }
            else
            {
                if (VerticalOrHorizontal)
                {
                    _splitter.PreviousPosition = (int)e.ClientY;
                    _splitter.PreviousPosition2 = (int)e.ClientX;
                }
                else
                {
                    _splitter.PreviousPosition = (int)e.ClientX;
                    _splitter.PreviousPosition2 = (int)e.ClientY;
                }
            }

            OnDragStart?.Invoke(Index, (int)e.ClientX, (int)e.ClientY);
        }

        private void OnPointerMove(PointerEventArgs e)
        {
            if (_dragMode)
            {
                if (e.Buttons == 1)
                {
                    int newPosition = 0;
                    int newPosition2 = 0;

                    if (IsDiagonal)
                    {
                        newPosition = (int)e.ClientX;
                        newPosition2 = (int)e.ClientY;

                        if (_splitter.PreviousPosition != newPosition || _splitter.PreviousPosition2 != newPosition2)
                        {
                            OnDiagonalPositionChange?.Invoke(Index, newPosition - _splitter.PreviousPosition, newPosition2 - _splitter.PreviousPosition2);

                            _splitter.PreviousPosition = newPosition;
                            _splitter.PreviousPosition2 = newPosition2;
                        }
                    }
                    else
                    {
                        if (VerticalOrHorizontal)
                        {
                            newPosition = (int)e.ClientY;
                            newPosition2 = (int)e.ClientX;
                        }
                        else
                        {
                            newPosition = (int)e.ClientX;
                            newPosition2 = (int)e.ClientY;
                        }

                        if (Math.Abs(_splitter.PreviousPosition2 - newPosition2) < 100)
                        {
                            if (_splitter.PreviousPosition != newPosition)
                            {
                                OnPositionChange?.Invoke(VerticalOrHorizontal, Index, newPosition - _splitter.PreviousPosition);

                                _splitter.PreviousPosition = newPosition;
                            }
                        }
                        //else
                        //{
                        //    //BsJsInterop.StopDrag(bSplitter.bsbSettings.ID);
                        //}
                    }
                }
            }
        }

        private void OnPointerUp(PointerEventArgs e)
        {
            JsInvokeAsync<bool>(JSInteropConstants.ReleasePointerCapture, Ref, e.PointerId);
            _dragMode = false;

            OnDragEnd?.Invoke(Index, (int)e.ClientX, (int)e.ClientY);
        }

        public void Dispose()
        {
        }

        public void SetColor(string c)
        {
            BgColor = c;
            EnableRender = true;
            StateHasChanged();
        }
    }
}
